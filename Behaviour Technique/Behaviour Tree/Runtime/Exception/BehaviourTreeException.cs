using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BehaviourTreeException : Exception
{
    public BehaviourTreeException(string message) : base(message) { }
}

