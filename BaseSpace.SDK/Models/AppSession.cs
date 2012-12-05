﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Illumina.BaseSpace.SDK.Models
{
    [DataContract( Name = "AppSession")]
    public class AppSessionCompact : AbstractResource
    {
        [DataMember(IsRequired = true)]
        public override string Id { get; set; }

        [DataMember(IsRequired = true)]
        public override Uri Href { get; set; }

        [DataMember]
        public ApplicationCompact Application { get; set; }

        [DataMember]
        public UserCompact UserCreatedBy { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string StatusSummary { get; set; }

        [DataMember]
        public DateTime DateCreated { get; set; }
    }


    [DataContract]
    public class AppSession : AppSessionCompact
    {
        [DataMember]
        public string OriginatingUri { get; set; }

        [DataMember]
        public IContentReference<IAbstractResource>[] References { get; set; }
    }
}
