using System;
using System.Collections.Generic;
using System.Text;

namespace Fingerprint.Infrastructure.Entities
{
    public class FingerprintDTO
    {
        public int SongId { get; set; }
        public long Hash { get; set; }
        public int TimeAnchor { get; set; }
    }
}
