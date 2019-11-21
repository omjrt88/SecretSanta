using System;
using System.Collections.Generic;

namespace SecretSanta
{
    public class Person
    {
        public Person()
        {
            Name = String.Empty;
            Target = String.Empty;
            Targeted = false;
        }

        public string Name { get; set; }

        public string Phone { get; set; }

        public List<string> Exclusions { get; set; }

        public string Target { get; set; }

        public bool Targeted { get; set; }

    }
}
