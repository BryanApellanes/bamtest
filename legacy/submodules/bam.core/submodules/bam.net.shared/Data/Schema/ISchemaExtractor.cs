/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Net.Data.Schema
{
    public interface ISchemaExtractor
    {
        SchemaNameMap NameMap { get; set; }
        SchemaDefinition Extract();
    }
}
