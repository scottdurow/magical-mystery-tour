import { contoso_listing_contoso_listing_contoso_features } from '../dataverse-gen/enums/contoso_listing_contoso_listing_contoso_features';

export interface ListingFeatures {
    Garden: boolean;
    Parking: boolean;
    Pool: boolean;
    Gym: boolean;
    Security: boolean;
}

// Create a const of lookups between contoso_listing_contoso_listing_contoso_features and the features object
export const FeatureLookup = {
    Parking: contoso_listing_contoso_listing_contoso_features.Parking,
    WashingMachine: contoso_listing_contoso_listing_contoso_features.WashingMachine,
    Pool: contoso_listing_contoso_listing_contoso_features.Pool,
    Garden: contoso_listing_contoso_listing_contoso_features.Garden,
    Gym: contoso_listing_contoso_listing_contoso_features.Gym,
    Security: contoso_listing_contoso_listing_contoso_features.Security,
};
