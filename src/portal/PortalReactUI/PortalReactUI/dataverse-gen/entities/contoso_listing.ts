/* eslint-disable*/
import { IEntity } from "dataverse-ify";
// Entity contoso_listing
export const contoso_listingMetadata = {
  typeName: "mscrm.contoso_listing",
  logicalName: "contoso_listing",
  collectionName: "contoso_listings",
  primaryIdAttribute: "contoso_listingid",
  attributeTypes: {
    // Numeric Types
    contoso_image_timestamp: "BigInt",
    contoso_pricepermonth: "Money",
    contoso_pricepermonth_base: "Money",
    exchangerate: "Decimal",
    importsequencenumber: "Integer",
    timezoneruleversionnumber: "Integer",
    utcconversiontimezonecode: "Integer",
    versionnumber: "BigInt",
    // Optionsets
    contoso_features: "MultiSelect",
    statecode: "Optionset",
    statuscode: "Optionset",
    // Date Formats
    createdon: "DateAndTime:UserLocal",
    modifiedon: "DateAndTime:UserLocal",
    overriddencreatedon: "DateOnly:UserLocal",
  },
  navigation: {
    contoso_PrimaryImage: ["mscrm.contoso_listingimage"],
    createdby: ["mscrm.systemuser"],
    createdonbehalfby: ["mscrm.systemuser"],
    modifiedby: ["mscrm.systemuser"],
    modifiedonbehalfby: ["mscrm.systemuser"],
    ownerid: ["mscrm.principal"],
    owningbusinessunit: ["mscrm.businessunit"],
    owningteam: ["mscrm.team"],
    owninguser: ["mscrm.systemuser"],
    transactioncurrencyid: ["mscrm.transactioncurrency"],
  },
};

// Attribute constants
export const enum contoso_listingAttributes {
  contoso_address = "contoso_address",
  contoso_description = "contoso_description",
  contoso_displayname = "contoso_displayname",
  contoso_features = "contoso_features",
  contoso_Image = "contoso_image",
  contoso_Image_Timestamp = "contoso_image_timestamp",
  contoso_Image_URL = "contoso_image_url",
  contoso_ImageId = "contoso_imageid",
  contoso_listingId = "contoso_listingid",
  contoso_listingid1 = "contoso_listingid1",
  contoso_name = "contoso_name",
  contoso_pricepermonth = "contoso_pricepermonth",
  contoso_pricepermonth_Base = "contoso_pricepermonth_base",
  contoso_PrimaryImage = "contoso_primaryimage",
  contoso_PrimaryImageName = "contoso_primaryimagename",
  CreatedBy = "createdby",
  CreatedByName = "createdbyname",
  CreatedByYomiName = "createdbyyominame",
  CreatedOn = "createdon",
  CreatedOnBehalfBy = "createdonbehalfby",
  CreatedOnBehalfByName = "createdonbehalfbyname",
  CreatedOnBehalfByYomiName = "createdonbehalfbyyominame",
  ExchangeRate = "exchangerate",
  ImportSequenceNumber = "importsequencenumber",
  ModifiedBy = "modifiedby",
  ModifiedByName = "modifiedbyname",
  ModifiedByYomiName = "modifiedbyyominame",
  ModifiedOn = "modifiedon",
  ModifiedOnBehalfBy = "modifiedonbehalfby",
  ModifiedOnBehalfByName = "modifiedonbehalfbyname",
  ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame",
  OverriddenCreatedOn = "overriddencreatedon",
  OwnerId = "ownerid",
  OwnerIdName = "owneridname",
  OwnerIdType = "owneridtype",
  OwnerIdYomiName = "owneridyominame",
  OwningBusinessUnit = "owningbusinessunit",
  OwningBusinessUnitName = "owningbusinessunitname",
  OwningTeam = "owningteam",
  OwningUser = "owninguser",
  statecode = "statecode",
  statuscode = "statuscode",
  TimeZoneRuleVersionNumber = "timezoneruleversionnumber",
  TransactionCurrencyId = "transactioncurrencyid",
  TransactionCurrencyIdName = "transactioncurrencyidname",
  UTCConversionTimeZoneCode = "utcconversiontimezonecode",
  VersionNumber = "versionnumber",
}
// Early Bound Interface
export interface contoso_listing extends IEntity {
  /*
  Address StringType
  */
  contoso_address?: string | null;
  /*
  Description StringType
  */
  contoso_description?: string | null;
  /*
  Display Name StringType
  */
  contoso_displayname?: string | null;
  /*
  Features contoso_listing_contoso_listing_contoso_features
  */
  contoso_features?: import("../enums/contoso_listing_contoso_listing_contoso_features").contoso_listing_contoso_listing_contoso_features[] | null;
  /*
  Image ImageType
  */
  contoso_image?: string | null;
  /*
   BigIntType
  */
  contoso_image_timestamp?: number | null;
  /*
   StringType
  */
  contoso_image_url?: string | null;
  /*
   UniqueidentifierType
  */
  contoso_imageid?: import("dataverse-ify").Guid | null;
  /*
  Listing UniqueidentifierType Unique identifier for entity instances
  */
  contoso_listingid?: import("dataverse-ify").Guid | null;
  /*
  Listing ID StringType
  */
  contoso_listingid1?: string | null;
  /*
  Name StringType
  */
  contoso_name?: string | null;
  /*
  Price per Month MoneyType
  */
  contoso_pricepermonth?: number | null;
  /*
  Price per Month (Base) MoneyType Value of the Price per Month in base currency.
  */
  contoso_pricepermonth_base?: number | null;
  /*
  Primary Image LookupType The primary image
  */
  contoso_primaryimage?: import("dataverse-ify").EntityReference | null;
  /*
   StringType
  */
  contoso_primaryimagename?: string | null;
  /*
  Created By LookupType Unique identifier of the user who created the record.
  */
  createdby?: import("dataverse-ify").EntityReference | null;
  /*
   StringType
  */
  createdbyname?: string | null;
  /*
   StringType
  */
  createdbyyominame?: string | null;
  /*
  Created On DateTimeType Date and time when the record was created. DateAndTime:UserLocal
  */
  createdon?: Date | null;
  /*
  Created By (Delegate) LookupType Unique identifier of the delegate user who created the record.
  */
  createdonbehalfby?: import("dataverse-ify").EntityReference | null;
  /*
   StringType
  */
  createdonbehalfbyname?: string | null;
  /*
   StringType
  */
  createdonbehalfbyyominame?: string | null;
  /*
  Exchange Rate DecimalType Exchange rate for the currency associated with the entity with respect to the base currency.
  */
  exchangerate?: number | null;
  /*
  Import Sequence Number IntegerType Sequence number of the import that created this record.
  */
  importsequencenumber?: number | null;
  /*
  Modified By LookupType Unique identifier of the user who modified the record.
  */
  modifiedby?: import("dataverse-ify").EntityReference | null;
  /*
   StringType
  */
  modifiedbyname?: string | null;
  /*
   StringType
  */
  modifiedbyyominame?: string | null;
  /*
  Modified On DateTimeType Date and time when the record was modified. DateAndTime:UserLocal
  */
  modifiedon?: Date | null;
  /*
  Modified By (Delegate) LookupType Unique identifier of the delegate user who modified the record.
  */
  modifiedonbehalfby?: import("dataverse-ify").EntityReference | null;
  /*
   StringType
  */
  modifiedonbehalfbyname?: string | null;
  /*
   StringType
  */
  modifiedonbehalfbyyominame?: string | null;
  /*
  Record Created On DateTimeType Date and time that the record was migrated. DateOnly:UserLocal
  */
  overriddencreatedon?: Date | null;
  /*
  Owner OwnerType Owner Id
  */
  ownerid?: import("dataverse-ify").EntityReference | null;
  /*
   StringType Name of the owner
  */
  owneridname?: string | null;
  /*
   EntityNameType Owner Id Type
  */
  owneridtype?: string | null;
  /*
   StringType Yomi name of the owner
  */
  owneridyominame?: string | null;
  /*
  Owning Business Unit LookupType Unique identifier for the business unit that owns the record
  */
  owningbusinessunit?: import("dataverse-ify").EntityReference | null;
  /*
   StringType
  */
  owningbusinessunitname?: string | null;
  /*
  Owning Team LookupType Unique identifier for the team that owns the record.
  */
  owningteam?: import("dataverse-ify").EntityReference | null;
  /*
  Owning User LookupType Unique identifier for the user that owns the record.
  */
  owninguser?: import("dataverse-ify").EntityReference | null;
  /*
  Status contoso_listing_contoso_listing_statecode Status of the Listing
  */
  statecode?: import("../enums/contoso_listing_contoso_listing_statecode").contoso_listing_contoso_listing_statecode | null;
  /*
  Status Reason contoso_listing_contoso_listing_statuscode Reason for the status of the Listing
  */
  statuscode?: import("../enums/contoso_listing_contoso_listing_statuscode").contoso_listing_contoso_listing_statuscode | null;
  /*
  Time Zone Rule Version Number IntegerType For internal use only.
  */
  timezoneruleversionnumber?: number | null;
  /*
  Currency LookupType Unique identifier of the currency associated with the entity.
  */
  transactioncurrencyid?: import("dataverse-ify").EntityReference | null;
  /*
   StringType
  */
  transactioncurrencyidname?: string | null;
  /*
  UTC Conversion Time Zone Code IntegerType Time zone code that was in use when the record was created.
  */
  utcconversiontimezonecode?: number | null;
  /*
  Version Number BigIntType Version Number
  */
  versionnumber?: number | null;
}
