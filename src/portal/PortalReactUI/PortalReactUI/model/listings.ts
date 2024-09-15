import { contoso_listing } from '../dataverse-gen/entities/contoso_listing';
import { contoso_ListingImage } from '../dataverse-gen/entities/contoso_ListingImage';

export interface ListingData {
    listing: contoso_listing;
    images: contoso_ListingImage[];
}
