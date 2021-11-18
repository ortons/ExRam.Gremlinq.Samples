﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExRam.Gremlinq.Core;

namespace ExRam.Gremlinq.Samples.Shared {
    public class Logic {
        private Person? _marko;
        private Person? _josh;
        private Person? _peter;
        private Person? _daniel;
        private Person? _vadas;

        private readonly TextWriter _writer;
        private readonly IGremlinQuerySource _g;

        public Logic(IGremlinQuerySource g, TextWriter writer) {
            _g = g;
            _writer = writer;
        }

        public async Task Run() {
            await Create_the_graph();
             
            await Create_vertices_and_a_relation_in_one_query();

            await Who_does_Marko_know();
            await Who_Is_Known_By_Both_Marko_And_Peter();
            await Who_is_older_than_30();
            await Whose_name_starts_with_B();
            await Who_knows_who();
            await Who_does_what();
            await What_pets_are_around();
            await How_many_pets_does_everybody_have();
            await Who_has_that_phone_number();
            await Who_has_a_phone();
            await What_entities_are_there();
            await Who_created_some_software();
            await Whose_age_is_29_30_or_31();

            await Set_and_get_metadata_on_Marko();
        }

        
        
        
        
        public async Task RunJustQueries() {
            
            
            
            await GetMeARandomNodeById();

            await GetMeAFewNodes();
            
            await Who_created_some_software();
            
            await What_entities_are_there();
        }
        
        
        
        
        
        private async Task Create_the_graph() {
            // Create a graph very similar to the one
            // found at http://tinkerpop.apache.org/docs/current/reference/#graph-computing.

            await _writer.WriteAsync("Clearing the database...");

            await _g.V().Drop();

            await _writer.WriteAsync("creating a new database...");

            _marko = await _g.AddV(new Person {
                                                  Id = IdGenerator.GetId(),
                                                  Name = "Marko",
                                                  Age = 29
                                              }).FirstAsync();

            _vadas = await _g.AddV(new Person {
                                                  Id = IdGenerator.GetId(),
                                                  Name = "Vadas",
                                                  Age = 27
                                              }).FirstAsync();

            _josh = await _g.AddV(new Person {
                                                 Id = IdGenerator.GetId(),
                                                 Name = "Josh",
                                                 Age = 32
                                             }).FirstAsync();

            _peter = await _g.AddV(new Person {
                                                  Id = IdGenerator.GetId(),
                                                  Name = "Peter",
                                                  Age = 35
                                              }).FirstAsync();

            _daniel = await _g.AddV(new Person {
                                                   Id = IdGenerator.GetId(),
                                                   Name = "Daniel",
                                                   Age = 37,
                                                   PhoneNumbers = new[] { "+491234567", "+492345678" }
                                               }).FirstAsync();

            var charlie = await _g.AddV(new Dog {
                                                    Id = IdGenerator.GetId(),
                                                    Name = "Charlie",
                                                    Age = 2
                                                }).FirstAsync();

            var catmanJohn = await _g.AddV(new Cat {
                                                       Id = IdGenerator.GetId(),
                                                       Name = "Catman John",
                                                       Age = 5
                                                   }).FirstAsync();

            var luna = await _g.AddV(new Cat {
                                                 Id = IdGenerator.GetId(),
                                                 Name = "Luna",
                                                 Age = 9
                                             }).FirstAsync();

            var lop = await _g.AddV(new Software {
                                                     Id = IdGenerator.GetId(),
                                                     Name = "Lop",
                                                     Language = ProgrammingLanguage.Java
                                                 }).FirstAsync();

            var ripple = await _g.AddV(new Software {
                                                        Id = IdGenerator.GetId(),
                                                        Name = "Ripple",
                                                        Language = ProgrammingLanguage.Java
                                                    }).FirstAsync();

            
            await _g.V(_marko.Id!).AddE<Knows>().To(__ => __.V(_vadas.Id!)).FirstAsync();

            await _g.V(_marko.Id!).AddE<Knows>().To(__ => __.V(_josh.Id!)).FirstAsync();

            await _g.V(_marko.Id!).AddE<Created>().To(__ => __.V(lop.Id!)).FirstAsync();

            await _g.V(_josh.Id!).AddE<Created>().To(__ => __.V(ripple.Id!)).FirstAsync();

            // Creates multiple edges in a single query
            // Note that query ends with ToArrayAsync

            await _g.V(_josh.Id!, _peter.Id!).AddE<Created>().To(__ => __.V(lop.Id!)).ToArrayAsync();

            await _g.V(_josh.Id!).AddE<Owns>().To(__ => __.V(charlie.Id!)).FirstAsync();

            await _g.V(_josh.Id!).AddE<Owns>().To(__ => __.V(luna.Id!)).FirstAsync();

            await _g.V(_daniel.Id!).AddE<Owns>().To(__ => __.V(catmanJohn.Id!)).FirstAsync();

            await _writer.WriteLineAsync("done.");
            await _writer.WriteLineAsync();
        }

        private async Task GetMeARandomNodeById() {

            var i = IdGenerator.GetRandomAllocatedId( );
            
            _writer.WriteLine($"Fetching vertex with id {i}");

            var v = await _g.V(i);

            DumpVertex(v, $"node id was {i}");


        }  
        
        private async Task GetMeAFewNodes() {


            int[] ids = new[] { 2, 4, 5, 8 };

            var v = await _g.V(ids.Select(f=>f.ToString()));

            DumpVertexRange(v);

        }
        private void DumpVertexRange(IEnumerable<object> v) {
            foreach (var o in v) {
                _writer.WriteLine($"vertex returned {ObjectDumper.Dump(o)}");
            }
            

        }

        private void DumpVertex(object o, string? message = null) {
            if (message == null)
            _writer.WriteLine($"vertex returned {ObjectDumper.Dump(o)}");
            else {
                _writer.WriteLine($"{message} {ObjectDumper.Dump(o)}");
            }
        }
        
        
        
        private async Task Create_vertices_and_a_relation_in_one_query() {
            // This demonstrates how to create 2 vertices and a connecting
            // edge between them in a single query.

            await _g.AddV(new Person {  Id = IdGenerator.GetId(),Name = "Bob", Age = 36 }).AddE<Knows>().To(__ => __.AddV(new Person {  Id = IdGenerator.GetId(),
                                                                                                               Name = "Jeff", Age = 27 })).FirstAsync();
        }

        private async Task Who_does_Marko_know() {
            // From Marko, walk all the 'Knows' edge to all the persons
            // that he knows and order them by their name.
            var knownPersonsToMarko = await _g.V(_marko!.Id!).Out<Knows>().OfType<Person>().Order(x => x.By(x => x.Name)).Values(x => x.Name!).ToArrayAsync();

            await _writer.WriteLineAsync("Who does Marko know?");

            foreach (var person in knownPersonsToMarko)
                await _writer.WriteLineAsync($" Marko knows {person}.");

            await _writer.WriteLineAsync();
        }

        private async Task Who_Is_Known_By_Both_Marko_And_Peter() {
            await _g.V(_peter!.Id!).AddE<Knows>().To(__ => __.V(_josh!.Id!)).FirstAsync();

            await _g.V(_peter.Id!).AddE<Knows>().To(__ => __.V(_daniel!.Id!)).FirstAsync();

            var whoIsKnownByBothMarkoAndPeter = await _g.V(_marko!.Id!).Both<Knows>().OfType<Person>().Fold().As((__, markosFriends)
                => __.V(_peter.Id!).Both<Knows>().OfType<Person>().Where(petersFriend => markosFriends.Value.Contains(petersFriend)).Dedup());

            await _writer.WriteLineAsync("Who is known by both Marko and Peter?");

            foreach (var people in whoIsKnownByBothMarkoAndPeter)
                await _writer.WriteLineAsync($" {people.Name} is known by both Marko and Peter");

            await _writer.WriteLineAsync();
        }

        private async Task Who_is_older_than_30() {
            // Gremlinq supports boolean expressions like you're used to use them
            // in your Linq-queries. Under the hood, they will be translated to the
            // corresponding Gremlin-expressions, like
            //
            //   "g.hasLabel('Person').has('Age', gt(30))"
            //
            // in this case.

            // Also, this sample demonstrates that instead of calling ToArrayAsync
            // and awaiting that, you may just await the IGremlinQuery<Person>!

            var personsOlderThan30 = await _g.V<Person>().Where(x => x.Age > 30);

            await _writer.WriteLineAsync("Who is older than 30?");

            foreach (var person in personsOlderThan30)
                await _writer.WriteLineAsync($" {person.Name} is older than 30.");

            await _writer.WriteLineAsync();
        }

        private async Task Whose_name_starts_with_B() {
            // This sample demonstrates the power of ExRam.Gremlinq! Even an expression
            // like 'StartsWith' on a string will be recognized by ExRam.Gremlinq and translated
            // to proper Gremlin syntax!

            var nameStartsWithB = await _g.V<Person>().Where(x => x.Name != null && x.Name.StartsWith("B")).ToArrayAsync();

            await _writer.WriteLineAsync("Whose name starts with 'B'?");

            foreach (var person in nameStartsWithB)
                await _writer.WriteLineAsync($" {person.Name}'s name starts with a 'B'.");

            await _writer.WriteLineAsync();
        }

        private async Task Who_knows_who() {
            // Here, we demonstrate how to deal with Gremlin step labels. Instead of
            // dealing with raw strings, ExRam.Gremlinq uses a dedicated 'StepLabel'-type
            // for these. And you don't even need to declare them upfront, as the
            // As-operator of ExRam.Gremlinq will put them in scope for you, along
            // with a continuation-query that you can further build upon!

            // Also, ExRam.Gremlinq's Select operators will not leave you with
            // raw dictionaries (or maps, as Java calls them). Instead, you'll get
            // nice ValueTuples!

            var friendTuples = await _g.V<Person>().As((__, person) => __.Out<Knows>().OfType<Person>().As((__, friend) => __.Select(person, friend)));

            await _writer.WriteLineAsync("Who knows who?");

            foreach (var (person1, person2) in friendTuples)
                await _writer.WriteLineAsync($" {person1.Name} knows {person2.Name}.");

            await _writer.WriteLineAsync();
        }

        private async Task Who_does_what() {
            // So far, we have only been dealing with vertices. ExRam.Gremlinq is cool
            // with edges too!

            var tuples = await _g.V<Person>().As((__, person) => __.OutE<Edge>().As((__, edge) => __.InV<Vertex>().As((__, what) => __.Select(person, edge, what))));

            await _writer.WriteLineAsync("Who does what?");

            foreach (var (person, does, what) in tuples)
                await _writer.WriteLineAsync($" {person.Name} {does.Label} a {what.Label}.");

            await _writer.WriteLineAsync();
        }

        private async Task What_pets_are_around() {
            // ExRam.Gremlinq supports inheritance! Below query will find all the dogs
            // and all the cats and instantiate the right type, as 'pet.GetType()' proves.

            var pets = await _g.V<Pet>();

            await _writer.WriteLineAsync("What pets are around?");

            foreach (var pet in pets)
                await _writer.WriteLineAsync($" There's a {pet.GetType().Name} named {pet.Name}.");

            await _writer.WriteLineAsync();
        }

        private async Task How_many_pets_does_everybody_have() {
            // This sample demonstrates how to fluently build projections with
            // ExRam.Gremlinq. It can project to a ValueTuple or to a dynamic.
            // In the latter case, the user may specify the name of each projection.

            await _writer.WriteLineAsync("How many pets does everybody have?");

            var dynamics = await _g.V<Person>().Project(b => b.ToDynamic().By(person => person.Name!).By("count", __ => __.Cast<object>().Out<Owns>().OfType<Pet>().Count()));

            foreach (var d in dynamics)
                await _writer.WriteLineAsync($" {d.Name} owns {d.count} pets.");

            await _writer.WriteLineAsync();
        }

        private async Task Who_has_that_phone_number() {
            // ExRam.Gremlinq supports multi-properties! And since these are
            // represented on the POCOs as arrays (in this case PhoneNumbers),
            // you want to call things like "Contains" on them! Surprise: ExRam.Gremlinq
            // recognizes these expressions!

            await _writer.WriteLineAsync("Who got the phone number +491234567 ?");

            var personWithThatPhoneNumber = await _g.V<Person>().Where(person => person.PhoneNumbers!.Contains("+491234567")).FirstOrDefaultAsync();

            await _writer.WriteLineAsync(personWithThatPhoneNumber != null ?
                $" {personWithThatPhoneNumber.Name} has a phone with the number +491234567" :
                " Nobody got a phone with the phone number +491234567");

            await _writer.WriteLineAsync();
        }

        private async Task Who_has_a_phone() {
            // Another example of an expression on the Person-POCO that ExRam.Gremlinq
            // recognizes:

            await _writer.WriteLineAsync("Who has got a phone?");

            var personsWithPhoneNumber = await _g.V<Person>().Where(person => person.PhoneNumbers!.Any());

            foreach (var person in personsWithPhoneNumber)
                await _writer.WriteLineAsync($" {person.Name} has a phone!");

            await _writer.WriteLineAsync();
        }

        private async Task What_entities_are_there() {
            // "Group" also has a beautiful fluent interface!

            await _writer.WriteLineAsync("What entities are there?");

            var entityGroups = await _g.V().Group(g => g.ByKey(__ => __.Label()).ByValue(__ => __.Count())).FirstAsync();

            foreach (var entityGroup in entityGroups)
                await _writer.WriteLineAsync(
                    $" There {(entityGroup.Value == 1 ? "is" : "are")} {entityGroup.Value} instance{(entityGroup.Value == 1 ? "" : "s")} of {entityGroup.Key}.");

            await _writer.WriteLineAsync();
        }

        private async Task Who_created_some_software() {
            // This showcases the power of the fluent interface of ExRam.Gremlinq.
            // Once we go from a 'Person' to the 'Created' edge, the entity we
            // came from is actually encoded in the interface, so on calling "OutV",
            // ExRam.Gremlinq remembers that we're now on "Persons" again.

            await _writer.WriteLineAsync("Who created some software?");

            var creators = await _g.V<Person>().OutE<Created>().OutV().Dedup();

            foreach (var creator in creators)
                await _writer.WriteLineAsync($" {creator.Name} created some software.");

            await _writer.WriteLineAsync();
        }

        private async Task Whose_age_is_29_30_or_31() {
            // ExRam.Gremlinq even defines extension methods on StepLabels so
            // you can ask question like the following: Which persons have an age
            // that's within a previously collected set of ages, referenced by a step label?
            //
            // So first, for simplicity, we inject 3 values (29, 30, 31), fold them
            // and store them in a step label 'ages'. Note that these values 29, 30 and 31
            // don't need to be hard coded but can come from an ordinary traversal.
            // Then, we ask for all the persons whose age is contained within the array
            // that the 'ages' step label references.

            await _writer.WriteLineAsync("Whose age is either 29, 30 or 31?");

            var personsWithSpecificAges = await _g.Inject(29, 30, 31).Fold().As((_, ages) => _.V<Person>().Where(person => ages.Value.Contains(person.Age)));

            foreach (var person in personsWithSpecificAges)
                await _writer.WriteLineAsync($" {person.Name}'s age is either 29, 30 or 31.");

            await _writer.WriteLineAsync();
        }

        private async Task Set_and_get_metadata_on_Marko() {
            // Demonstrates setting and retrieving properties on vertex properties.
            // Furthermore, we demonstrate how to dynamically avoid queries if the
            // underlying graph database provider doesn't support them. In this case,
            // this will not run on AWS Neptune since it doesn't support meta properties.

            if (_g.Environment.FeatureSet.Supports(VertexFeatures.MetaProperties)) {
           //     await _g.V<Person>(_marko!.Id!).Properties(x => x.Name!).Property(x => x.Creator, "Stephen").Property(x => x.Date, DateTimeOffset.Now).ToArrayAsync();

                var metaProperties = await _g.V().Properties().Properties().ToArrayAsync();

                await _writer.WriteLineAsync("Meta properties on Vertex properties:");

                foreach (var metaProperty in metaProperties)
                    await _writer.WriteLineAsync($" {metaProperty}");

                await _writer.WriteLineAsync();
            }
        }
    }
}
