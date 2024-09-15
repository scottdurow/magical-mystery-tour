/* eslint-disable*/
import { IEntity } from "dataverse-ify";
// Entity contoso_payment
export const contoso_paymentMetadata = {
  typeName: "mscrm.contoso_payment",
  logicalName: "contoso_payment",
  collectionName: "contoso_payments",
  primaryIdAttribute: "contoso_paymentid",
  attributeTypes: {
    // Numeric Types
    contoso_amount: "Decimal",
    // Optionsets
    contoso_provider: "Optionset",
    contoso_status: "Optionset",
    // Date Formats
    contoso_createdon: "DateOnly:UserLocal",
  },
  navigation: {
  },
};

// Attribute constants
export const enum contoso_paymentAttributes {
  contoso_Amount = "contoso_amount",
  contoso_CreatedOn = "contoso_createdon",
  contoso_CurrencyCode = "contoso_currencycode",
  contoso_name = "contoso_name",
  contoso_paymentId = "contoso_paymentid",
  contoso_Provider = "contoso_provider",
  contoso_Status = "contoso_status",
  contoso_UserId = "contoso_userid",
}
// Early Bound Interface
export interface contoso_payment extends IEntity {
  /*
  Amount [Required] DecimalType
  */
  contoso_amount?: number;
  /*
  Created On [Required] DateTimeType DateOnly:UserLocal
  */
  contoso_createdon?: Date;
  /*
  Currency Code [Required] StringType
  */
  contoso_currencycode?: string;
  /*
  Name StringType The name of the custom entity.
  */
  contoso_name?: string | null;
  /*
  Payment UniqueidentifierType Unique identifier for entity instances
  */
  contoso_paymentid?: import("dataverse-ify").Guid | null;
  /*
  Provider [Required] contoso_payment_contoso_payment_contoso_provider
  */
  contoso_provider?: import("../enums/contoso_payment_contoso_payment_contoso_provider").contoso_payment_contoso_payment_contoso_provider;
  /*
  Status [Required] contoso_payment_contoso_payment_contoso_status
  */
  contoso_status?: import("../enums/contoso_payment_contoso_payment_contoso_status").contoso_payment_contoso_payment_contoso_status;
  /*
  User Id [Required] StringType
  */
  contoso_userid?: string;
}
