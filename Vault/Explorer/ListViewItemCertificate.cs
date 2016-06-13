﻿using Microsoft.Azure.KeyVault;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Microsoft.PS.Common.Vault.Explorer
{
    /// <summary>
    /// Key Vault Certificate list view item which also presents itself nicely to PropertyGrid
    /// </summary>
    public class ListViewItemCertificate : ListViewItemBase
    {
        public readonly CertificateAttributes Attributes;
        public readonly string Thumbprint;

        private ListViewItemCertificate(ISession session, CertificateIdentifier identifier, CertificateAttributes attributes, string thumbprint, IDictionary<string, string> tags) : 
            base(session, KeyVaultCertificatesGroup,
                identifier, tags, attributes.Enabled, attributes.Created, attributes.Updated, attributes.NotBefore, attributes.Expires)
        {
            Attributes = attributes;
            Thumbprint = thumbprint;
        }

        public ListViewItemCertificate(ISession session, ListCertificateResponseMessage c) : this(session, c.Identifier, c.Attributes, c.X5T, c.Tags) { }

        public ListViewItemCertificate(ISession session, CertificateBundle cb) : this(session, cb.Id, cb.Attributes, cb.X5T, cb.Tags) { }

        protected override IEnumerable<PropertyDescriptor> GetCustomProperties()
        {
            yield return new ReadOnlyPropertyDescriptor("Content Type", CertificateContentType.Pfx);
            yield return new ReadOnlyPropertyDescriptor("Thumbprint", Thumbprint);
        }

        public override async Task<ListViewItemBase> ToggleAsync(CancellationToken cancellationToken)
        {
            CertificateBundle cb = await Session.CurrentVault.UpdateCertificateAsync(Name, new CertificateAttributes() { Enabled = !Attributes.Enabled }, Utils.AddChangedBy(Tags), cancellationToken); // Toggle only Enabled attribute
            return new ListViewItemCertificate(Session, cb);
        }

        public override async Task<ListViewItemBase> DeleteAsync(CancellationToken cancellationToken)
        {
            await Session.CurrentVault.DeleteCertificateAsync(Name, cancellationToken);
            return this;
        }
    }
}