using System.ComponentModel.DataAnnotations;

namespace Application.Features.Containers.CreateContainer;

public class CreateContainerCommand
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;
}
