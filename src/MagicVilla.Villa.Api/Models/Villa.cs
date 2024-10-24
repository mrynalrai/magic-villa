using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagicVilla.Villa.Api.Models
{
    public class Villa
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}