namespace AdminPanel.Models
{
    // Model som repræsenterer en bruger
    public class User
    {
        // Primær nøgle i databasen
        public int Id { get; set; }

        // Brugerens navn
        public string Name { get; set; }

        // Brugerens email
        public string Email { get; set; }

        // Brugerens rolle
        // Fx User eller Admin
        public Role Role { get; set; }

        // Firebase UID
        // Bruges til at forbinde Firebase login
        // med lokal SQL bruger
        public string Uid { get; set; }
    }
}