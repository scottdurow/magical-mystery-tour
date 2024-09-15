import { ApiError, ApiErrorCodes } from './apiErrors';
import { invokeFlow } from './invokeFlow';
import { XrmContextDataverseClient } from 'dataverse-ify/lib/dataverse-ify/DataverseClient/XrmContextDataverseClient';
import { contoso_listing } from '../dataverse-gen/entities/contoso_listing';
import { contoso_ListingImage } from '../dataverse-gen/entities/contoso_ListingImage';
import { ListingData } from '../model/listings';

export async function reserveListing(
    listingID: string,
    from: Date,
    to: Date,
    guests: number,
): Promise<{ sessionUrl: string }> {
    const parameters = {
        operation: 'checkout',
        listingID,
        from: from.toISOString().split('T')[0],
        to: to.toISOString().split('T')[0],
        guests,
        reservationId: '',
    };

    try {
        const responseData = await invokeFlow<{ sessionurl: string; errormessage: string }>(
            window.CRE_SETTING_CHECKOUT_FLOW_URL,
            parameters,
        );

        if (responseData.errormessage && responseData.errormessage.length > 0) {
            throw new ApiError(responseData.errormessage, ApiErrorCodes.API_CHECKOUT_ERROR);
        }
        return { sessionUrl: responseData.sessionurl };
    } catch (error) {
        if (error instanceof TypeError) {
            console.error('Error in checkout', error);
            throw new ApiError(error.message, ApiErrorCodes.NETWORK_ERROR);
        }
        throw error;
    }
}

export async function completeReservation(reservationId: string): Promise<void> {
    const parameters = {
        operation: 'complete',
        listingID: '',
        from: '',
        to: '',
        guests: 0,
        reservationId,
    };

    try {
        const responseData = await invokeFlow<{ errormessage: string }>(
            window.CRE_SETTING_CHECKOUT_FLOW_URL,
            parameters,
        );

        if (responseData.errormessage && responseData.errormessage.length > 0) {
            throw new ApiError(responseData.errormessage, ApiErrorCodes.API_CHECKOUT_ERROR);
        }
    } catch (error) {
        if (error instanceof TypeError) {
            console.error('Error in checkout', error);
            throw new ApiError(error.message, ApiErrorCodes.NETWORK_ERROR);
        }
        throw error;
    }
}

export async function getListings(serviceClient: XrmContextDataverseClient) {
    const fetch = `<fetch version="1.0" mapping="logical"
  no-lock="false" distinct="true">
  <entity name="contoso_listing">
      <attribute name="contoso_listingid" />
      <attribute name="contoso_image_url" />
      <attribute name="contoso_name" />
      <attribute name="contoso_address" />
      <attribute name="contoso_pricepermonth" />
      <attribute name="contoso_features" />
      <attribute name="contoso_displayname" />
      <attribute name="contoso_description" />
      <attribute name="contoso_primaryimage" />
      <filter type="and">
          <condition attribute="statecode" operator="eq" value="0" />
      </filter>
      <order attribute="contoso_name" descending="false" />
  </entity>
</fetch>`;
    const response = await serviceClient.retrieveMultiple<contoso_listing>(fetch);

    return response.entities;
}

export async function getListing(serviceClient: XrmContextDataverseClient, listingId: string): Promise<ListingData> {
    const listingFetch = `<fetch version="1.0" mapping="logical"
  no-lock="false" distinct="true">
  <entity name="contoso_listing">
      <attribute name="contoso_listingid" />
      <attribute name="contoso_listingid1" />
      <attribute name="contoso_image_url" />
      <attribute name="contoso_name" />
      <attribute name="contoso_address" />
      <attribute name="contoso_pricepermonth" />
      <attribute name="contoso_features" />
      <attribute name="contoso_displayname" />
      <attribute name="contoso_description" />
      <attribute name="contoso_primaryimage" />
      <filter type="and">
      <condition attribute="contoso_listingid" operator="eq" value="${listingId}" />
      </filter>
      <order attribute="contoso_name" descending="false" />
  </entity>
</fetch>`;
    const imagesFetch = `<fetch version="1.0" mapping="logical"
no-lock="false" distinct="true">
<entity name="contoso_listingimage">
<attribute name="contoso_listingimageid" />
<attribute name="contoso_name" />
<filter>
  <condition attribute="contoso_listing" operator="eq" value="${listingId}" />
</filter>
</entity>
</fetch>`;

    const [listing, images] = await Promise.all([
        serviceClient.retrieveMultiple<contoso_listing>(listingFetch),
        serviceClient.retrieveMultiple<contoso_ListingImage>(imagesFetch),
    ]);

    if (listing.entities.length === 0) {
        throw new Error(`Listing with id ${listingId} not found`);
    }

    return { listing: listing.entities[0], images: images.entities };
}
