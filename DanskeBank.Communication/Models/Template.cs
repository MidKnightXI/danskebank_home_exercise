using System.ComponentModel.DataAnnotations;

namespace DanskeBank.Communication.Models;

public class Template
{
    [MinLength(2), MaxLength(64)]
    public required string Name { get; set; }
    [MinLength(3), MaxLength(128)]
    public required string Subject { get; set; }
    [MinLength(0), MaxLength(1024)]
    public required string Body { get; set; }
}