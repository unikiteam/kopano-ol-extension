﻿// Copyright 2012 - Johan de Koning (johan@johandekoning.nl)
// 
// This file is part of WBXML .Net Library.
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal in 
// the Software without restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
// Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies 
// or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
// USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// The WAP Binary XML (WBXML) specification is developed by the 
// Open Mobile Alliance (http://www.openmobilealliance.org/)
// Details about this specification can be found at
// http://www.openmobilealliance.org/tech/affiliates/wap/wap-192-wbxml-20010725-a.pdf
//
// The ActiveSync WAP Binary XML (MS-ASWBXML) specification is 
// developed by Microsoft (http://www.microsoft.com/)
// Details about this specification can be found at 
// http://msdn.microsoft.com/en-us/library/dd299442.aspx

namespace Acacia.WBXML.ActiveSync
{
    internal class ResolveRecipientsCodePage : TagCodePage
    {
        public ResolveRecipientsCodePage()
        {
            AddToken(0x05, "ResolveRecipients");
            AddToken(0x06, "Response");
            AddToken(0x07, "Status");
            AddToken(0x08, "Type");
            AddToken(0x09, "Recipient");
            AddToken(0x0A, "DisplayName");
            AddToken(0x0B, "EmailAddress");
            AddToken(0x0C, "Certificates");
            AddToken(0x0D, "Certificate");
            AddToken(0x0E, "MiniCertificate");
            AddToken(0x0F, "Options");
            AddToken(0x10, "To");
            AddToken(0x11, "CertificateRetrieval");
            AddToken(0x12, "RecipientCount");
            AddToken(0x13, "MaxCertificates");
            AddToken(0x14, "MaxAmbiguousRecipients");
            AddToken(0x15, "CertificateCount");
            AddToken(0x16, "Availability");
            AddToken(0x17, "StartTime");
            AddToken(0x18, "EndTime");
            AddToken(0x19, "MergedFreeBusy");
            AddToken(0x1A, "Picture");
            AddToken(0x1B, "MaxSize");
            AddToken(0x1C, "Data");
            AddToken(0x1D, "MaxPictures");
        }
    }
}