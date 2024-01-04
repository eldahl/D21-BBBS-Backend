using System;
using System.Collections.Generic;

namespace BBBSBackend.DBModels;

public partial class LanguageStringTable
{
    public int Id { get; set; }

    public Guid UniqueIdentifier { get; set; }

    public string LangCode { get; set; } = null!;

    public string DataString { get; set; } = null!;
}
