/* eslint-disable*/
import { contoso_listingMetadata } from "./entities/contoso_listing";
import { contoso_listingimageMetadata } from "./entities/contoso_ListingImage";
import { contoso_paymentMetadata } from "./entities/contoso_payment";

export const Entities = {
  contoso_listing: "contoso_listing",
  contoso_ListingImage: "contoso_listingimage",
  contoso_payment: "contoso_payment",
};

// Setup Metadata
// Usage: setMetadataCache(metadataCache);
export const metadataCache = {
  entities: {
    contoso_listing: contoso_listingMetadata,
    contoso_listingimage: contoso_listingimageMetadata,
    contoso_payment: contoso_paymentMetadata,
  },
  actions: {
  }
};