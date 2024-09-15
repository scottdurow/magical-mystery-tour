using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentVirtualTableProvider
{

  public static class Int2GuidExtensions
  {
    // Convert an int to a Guid
    public static Guid ToGuid(this int value)
    {
      byte[] bytes = new byte[16];
      BitConverter.GetBytes(value).CopyTo(bytes, 0);
      return new Guid(bytes);
    }

  }

  public static class GuidExtensions
  {
    // Convert a Guid to an int
    public static int ToInt(this Guid value)
    {
      byte[] bytes = value.ToByteArray();
      return BitConverter.ToInt32(bytes, 0);
    }

  }

}
