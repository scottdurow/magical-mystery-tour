#pragma warning disable CS1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Contoso
{
	
	
	[System.Runtime.Serialization.DataContractAttribute()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.6")]
	public enum contoso_payment_contoso_provider
	{
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Stripe = 1,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		PayPal = 2,
	}
	
	[System.Runtime.Serialization.DataContractAttribute()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.6")]
	public enum contoso_payment_contoso_status
	{
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Pending = 1,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Active = 2,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Cancelled = 3,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Complete = 4,
	}
	
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("contoso_payment")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.6")]
	public partial class contoso_payment : Microsoft.Xrm.Sdk.Entity
	{
		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		public contoso_payment() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "contoso_payment";
		
		public const string EntityLogicalCollectionName = "contoso_payments";
		
		public const string EntitySetName = "contoso_payments";
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_amount")]
		public System.Nullable<decimal> contoso_Amount
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<decimal>>("contoso_amount");
			}
			set
			{
				this.SetAttributeValue("contoso_amount", value);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_createdon")]
		public System.Nullable<System.DateTime> contoso_CreatedOn
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("contoso_createdon");
			}
			set
			{
				this.SetAttributeValue("contoso_createdon", value);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_currencycode")]
		public string contoso_CurrencyCode
		{
			get
			{
				return this.GetAttributeValue<string>("contoso_currencycode");
			}
			set
			{
				this.SetAttributeValue("contoso_currencycode", value);
			}
		}
		
		/// <summary>
		/// The name of the custom entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_name")]
		public string contoso_name
		{
			get
			{
				return this.GetAttributeValue<string>("contoso_name");
			}
			set
			{
				this.SetAttributeValue("contoso_name", value);
			}
		}
		
		/// <summary>
		/// Unique identifier for entity instances
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_paymentid")]
		public System.Nullable<System.Guid> contoso_paymentId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("contoso_paymentid");
			}
			set
			{
				this.SetAttributeValue("contoso_paymentid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_paymentid")]
		public override System.Guid Id
		{
			get
			{
				return base.Id;
			}
			set
			{
				this.contoso_paymentId = value;
			}
		}
		
		/// <summary>
		/// The user who placed the payment
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_portaluser")]
		public Microsoft.Xrm.Sdk.EntityReference contoso_PortalUser
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("contoso_portaluser");
			}
			set
			{
				this.SetAttributeValue("contoso_portaluser", value);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_provider")]
		public virtual contoso_payment_contoso_provider? contoso_Provider
		{
			get
			{
				return ((contoso_payment_contoso_provider?)(EntityOptionSetEnum.GetEnum(this, "contoso_provider")));
			}
			set
			{
				this.SetAttributeValue("contoso_provider", value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_reservation")]
		public Microsoft.Xrm.Sdk.EntityReference contoso_Reservation
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("contoso_reservation");
			}
			set
			{
				this.SetAttributeValue("contoso_reservation", value);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_status")]
		public virtual contoso_payment_contoso_status? contoso_Status
		{
			get
			{
				return ((contoso_payment_contoso_status?)(EntityOptionSetEnum.GetEnum(this, "contoso_status")));
			}
			set
			{
				this.SetAttributeValue("contoso_status", value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contoso_userid")]
		public string contoso_UserId
		{
			get
			{
				return this.GetAttributeValue<string>("contoso_userid");
			}
			set
			{
				this.SetAttributeValue("contoso_userid", value);
			}
		}
	}
}
#pragma warning restore CS1591
