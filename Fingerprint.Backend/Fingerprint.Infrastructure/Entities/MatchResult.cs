using System;
using System.Collections.Generic;
using System.Text;

namespace Fingerprint.Infrastructure.Entities
{
    public class MatchResult
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public int DbTime { get; set; }
        public long Hash { get; set; }
    }
}
