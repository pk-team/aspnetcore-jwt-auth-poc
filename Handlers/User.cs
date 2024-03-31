namespace App;

public record User(string Name, string Email, IEnumerable<string> Roles);
