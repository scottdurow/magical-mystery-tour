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

namespace ContosoRealEstate.BusinessLogic.Models
{
	
	
	[System.Runtime.Serialization.DataContractAttribute(Namespace="http://schemas.microsoft.com/xrm/2011/new/")]
	[Microsoft.Xrm.Sdk.Client.RequestProxyAttribute("contoso_IsListingAvailableAPI")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.11")]
	public partial class contoso_IsListingAvailableAPIRequest : Microsoft.Xrm.Sdk.OrganizationRequest
	{
		
		public string To
		{
			get
			{
				if (this.Parameters.Contains("To"))
				{
					return ((string)(this.Parameters["To"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["To"] = value;
			}
		}
		
		public string From
		{
			get
			{
				if (this.Parameters.Contains("From"))
				{
					return ((string)(this.Parameters["From"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["From"] = value;
			}
		}
		
		public string ExcludeReservation
		{
			get
			{
				if (this.Parameters.Contains("ExcludeReservation"))
				{
					return ((string)(this.Parameters["ExcludeReservation"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["ExcludeReservation"] = value;
			}
		}
		
		public string ListingID
		{
			get
			{
				if (this.Parameters.Contains("ListingID"))
				{
					return ((string)(this.Parameters["ListingID"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["ListingID"] = value;
			}
		}
		
		public contoso_IsListingAvailableAPIRequest()
		{
			this.RequestName = "contoso_IsListingAvailableAPI";
			this.To = default(string);
			this.From = default(string);
			this.ExcludeReservation = default(string);
			this.ListingID = default(string);
		}
	}
	
	[System.Runtime.Serialization.DataContractAttribute(Namespace="http://schemas.microsoft.com/xrm/2011/new/")]
	[Microsoft.Xrm.Sdk.Client.ResponseProxyAttribute("contoso_IsListingAvailableAPI")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.11")]
	public partial class contoso_IsListingAvailableAPIResponse : Microsoft.Xrm.Sdk.OrganizationResponse
	{
		
		public contoso_IsListingAvailableAPIResponse()
		{
		}
		
		public bool Available
		{
			get
			{
				if (this.Results.Contains("Available"))
				{
					return ((bool)(this.Results["Available"]));
				}
				else
				{
					return default(bool);
				}
			}
		}
	}
}
#pragma warning restore CS1591