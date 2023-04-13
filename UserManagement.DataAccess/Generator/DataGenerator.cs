using Bogus;
using UserManagement.DataAccess.Data;
using UserManagement.Domain.Entities;

namespace UserManagement.DataAccess.Generator.DataGenerator
{
    public class DataGenerator
    {
        private readonly Faker<User> _fakeUser;
        private readonly ApplicationDbContext _context;

        public DataGenerator(ApplicationDbContext context)
        {
            _context = context;

            Randomizer.Seed = new Random(Seed: 123);

            _fakeUser = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.UserName, f => f.Person.UserName)
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.PasswordHash, f => "password") // Replace with your own logic for hashing passwords
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
            .RuleFor(u => u.Age, f => f.Random.Int(min: 18, max: 65))
            .RuleFor(u => u.Gender, f => f.PickRandom(new[] { "Male", "Female" }))
            .RuleFor(u => u.MaritalStatus, f => f.PickRandom(new[] { "Single", "Married", "Divorced", "Widowed" }))
            .RuleFor(u => u.Location, f => f.Address.City());

        }

        public void GenerateAndInsertFakeUsers(int count)
        {
            var users = _fakeUser.Generate(1000);

            _context.Users.UpdateRange(users);
            _context.SaveChanges();
        }
    }
}
