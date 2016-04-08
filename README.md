# LibAAS
Another sample project/workshop showing event sourcing in F#. Hopefully the exercises teach you both event sourcing and F#. 

Start with ex1 and work your way through to get a better understanding of how it can be to work with an application based on CQRS and event sourcing.

The language in the application is F#, mainly because event sourcing is functional and also to spread the beautiful word of F#.

The fictive domain that will be used is library as a service, where you will be able to register items as well as loan them. It shouldn't be that hard to implement more features, but that is enough for this sample tutorial.

Everything should work on Linux, OSX or Windows. To run on non Windows you can run `build.sh Ex1Start` to build and test the first exercise. Targets you can start are `Ex1Start`,`Ex1Done`,`Ex3Start`,`Ex2Done`,`Ex3Start`,`Ex3Done`,`Ex4Start` and `Ex4Done`. On Windows you can build from Visual Studio or run `build.cmd` with the same arguments as for `build.sh`.

You can go straight to [exercise 1](ex1/README.md) or read some background things information first.

## CQRS + event sourcing, the short explanation

CQRS stands for Command Query Responsibility Segregation, what it means is that you separate your reads from your writes. You don't have to use event sourcing to do so, but it is a good fit.

Event sourcing is the act of storing state changes as events instead of the actual state, and then use those events to rebuild the state when you need it.

With this in mind the overall architecture of our application will look like:

```
    command -> (validation) -> read events -> build state -> execute -> save events
```

## Basic concepts

Since we are using a functional language, we will not deal with state in the same way as you do in a language like C# or java. The state of the aggregates will be calculated by reading all events and then use those to *evolve* the state. You can see it is a state machine where each event take us to a new state. In pseudo code it will look like:

```
s0 = init state
s1 = evolve s0 event0
s2 = evolve s1 event1
...
sn = evolve sn-1 eventn-1
```

For those familiar with functional programming will recognize that this is the definition a fold operation or in C#/linq it is called aggregate.

```
sn = fold evolve "init state" events
```

What fold does is applying the function `evolve` to all the events in the list and passing the new state in as input to the next step. The argument `init state` is what is used as base state.
