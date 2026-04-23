using LoveSync.Models;
using System.Collections.Generic;
using System.Linq;

namespace LoveSync.Services
{
    public class IdeaService
    {
        // Film ötletek:
        private static List<Idea> _dummyMovies = new List<Idea>
        {
            new Idea
            {
                Title = "Eredet (Inception)",
                Description = "Álom az álomban... Christopher Nolan klasszikusa.",
                Category = "Movie",
                ImageUrl = "https://images.unsplash.com/photo-1606112219348-204d7d8b94ee?auto=format&fit=crop&w=800&q=80"
            },
            new Idea
            {
                Title = "A Sötét Lovag",
                Description = "Batman szembenéz Jokerral a sötét Gotham városában.",
                Category = "Movie",
                ImageUrl = "https://images.unsplash.com/photo-1614032686043-0b4c579b7b03?auto=format&fit=crop&w=800&q=80"
            },
            new Idea
            {
                Title = "Csillagok között",
                Description = "Űrutazás, szerelem és idő paradoxonok – az emberiség jövőjéért.",
                Category = "Movie",
                ImageUrl = "https://images.unsplash.com/photo-1454789548928-9efd52dc4031?auto=format&fit=crop&w=800&q=80"
            }
        };

        // Kaja ötletek:
        private static List<Idea> _foodIdeas = new List<Idea>
        {
            new Idea
            {
                Title = "Olasz pizzázás",
                Description = "Ropogós, kemencés pizza vékony tésztával és friss bazsalikommal.",
                Category = "Food",
                ImageUrl = "food_pizza.png"
            },
            new Idea
            {
                Title = "Sushi est",
                Description = "Friss halak, tekercsek és egy kis szaké. Japán hangulatban.",
                Category = "Food",
                ImageUrl = "food_sushi.png"
            },
            new Idea
            {
                Title = "Hamburger & Sör",
                Description = "Szaftos kézműves burger, ropogós krumpli és hideg sör.",
                Category = "Food",
                ImageUrl = "food_burger.png"
            },
            new Idea
            {
                Title = "Mexikói taco",
                Description = "Csípős, fűszeres ízek, guacamole és lime a tökéletes estére.",
                Category = "Food",
                ImageUrl = "food_taco.png"
            },
            new Idea
            {
                Title = "Otthoni főzés",
                Description = "Közös főzés és nevetés a konyhában.",
                Category = "Food",
                ImageUrl = "food_cooking.png"
            }
        };

        // Randi ötletek:
        private static List<Idea> _dateIdeas = new List<Idea>
        {
            new Idea
            {
                Title = "Séta a parkban",
                Description = "Kikapcsolódás a természetben, séta kézenfogva.",
                Category = "Date",
                ImageUrl = "date_park.png"
            },
            new Idea
            {
                Title = "Mozi est",
                Description = "Nézzünk meg egy filmet a nagyvásznon, popcornnal és öleléssel.",
                Category = "Date",
                ImageUrl = "date_cinema.png"
            },
            new Idea
            {
                Title = "Bowling",
                Description = "Játékos verseny, ki gurít jobban? Jó hangulat garantált.",
                Category = "Date",
                ImageUrl = "date_bowling.png"
            },
            new Idea
            {
                Title = "Szabadulószoba",
                Description = "Oldjunk meg rejtélyeket közösen, igazi csapatként!",
                Category = "Date",
                ImageUrl = "date_escape_room.png"
            },
            new Idea
            {
                Title = "Naplemente nézés",
                Description = "Keressünk egy kilátót és nézzük együtt a naplementét.",
                Category = "Date",
                ImageUrl = "date_sunset.png"
            }
        };

        public List<Idea> GetIdeas() => _dummyMovies;
        public List<Idea> GetFoodIdeas() => _foodIdeas;
        public List<Idea> GetDateIdeas() => _dateIdeas;

        public Idea GetIdeaByTitle(string title)
        {
            var all = new List<Idea>();
            all.AddRange(_dummyMovies);
            all.AddRange(_foodIdeas);
            all.AddRange(_dateIdeas);

            return all.FirstOrDefault(i => i.Title == title);
        }
    }
}
