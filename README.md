# AI Project

## Summary

- [AI Project](#ai-project)
  - [Summary](#summary)
  - [Contributors](#contributors)
  - [Presentation](#presentation)
- [Features](#features)
  - [Follow Player](#follow-player)
  - [Support Shoot](#support-shoot)
  - [Protect Player](#protect-player)
  - [Heal Player](#heal-player)
  - [Barrage Fire](#barrage-fire)
- [How to launch the project](#how-to-launch-the-project)
- [How we did thi](#how-we-did-this)
  - [Finite State Machine](#finite-state-machine)
  - [Squad Controller](#squad-controller)
      -[Formations](#formations)
      -[Prioritization](#prioritization)
  - [Limits and Improvements](#limits-of-our-fsm-- and-possible-improvements)

## Contributors

- [Felix](https://github.com/fBecaud?tab=overview&from=2024-09-01&to=2024-09-30%E2%80%8B%E2%80%8B%E2%80%8B)
- [Paul](https://github.com/Susanoo1004)
  
## Presentation

This is a project we had to do in our school in our 3rd year, where we had to make a basic AI.

# Features

## Follow Player

The AI agent can follow the player if he is nearby and will walk next to them in a formation with the other agents. When the agent arrives at his position, they stops and goes idle until the player starts moving again.

## Support Shoot

If the player shoots then all the agents that are not healing or protecting the player will start to shoot with the player to support them.

## Protect Player

If the player is going to receive a bullet, then an agent will start to move to go in front of the player to protect them from the bullets. The protector will also fight back by shooting a bullet towards the enemy. The agent will go blue when it is in this state.

![title](Screenshots/Protect.png)

## Heal Player

If the player is critically injured and needs to be healed, then an agent will start to go next to the player to heal them. The agent will go green when they is in this state.

![title](Screenshots/Heal.png)

## Barrage Fire

If the player decides to right-click this will activate the barrage fire, where all the ally agents will stop and shoot where you right-click until you click another time to deactivate it.

![title](Screenshots/Barrage.png)

# How to launch the project

First, download the project either from this repository or from another source. Then open the project with Unity Hub with the Unity Version 2022.3.47. Once this is done, open the "level" scene , and then play start.

# How we did this

## Finite State Machine 

It has been made very modulable, even though the transitions have to be set in code directly. It is templated on an enum EState that regroups every possible state. Each state is part of this enum.
The state is responsible for its own Exit, with the next State. It executes code at Enter, Update, and Exit and can also use a trigger for detection.
The FSM is responsible for the transition, and registering the current state. If the next one is different from the current one, it will be changed on the Update (delta time is tweakable). 
ChangeState() can be accessed outside of the FSM to force a behavior.

The main challenge we imposed ourselves was to rely the least upon updates for transition. Each time it is possible, we use events or Coroutines to set up a state change (player moving, player damaged, player in critical situation...).
This performance issue has a cost in readability and difficulty to use.

In its current shape, every state goes back to Idle.


The possible States are:
<pre>
       Protect
         ⭥
Follow⭤  Idle ⭤ Barrage
         ⭥  ⭥
       Heal Support
</pre>

## Squad Controller

Both the relative positions and the squad's overall behavior fall under his control. It redirects each agent into positions according to the Squad formation. It also decides which agent goes to heal or protect. Logically, it is also responsible for coordinated orders like barrage.
It has been reshaped several times not to need any player, and now each Agent derives from ISquadLeader and can be in charge of the squad.

### Formations

Possible (scalable) formations are square and circle formations.

### Prioritization

Protectors and Healers are chosen by a priority system relying on 1/their role, 2/ their distance to the player. If too far the candidate is eliminated, if, in range, priority goes to the corresponding role with a bonus, then the closest from the target.

## Limits of our FSM and possible improvements

The Finite State Machine is limiting the override by the state barrage; Either each state needs to include a transition to Barrage or the AI director needs to order the FSM to change. We opted for the latter.  
FSM is also limiting evolution with more states, requiring more maintenance and attention, not to be softlocked.
For our AI we should had 3 different Prefabs for a tank, an attacker, and a medic where you could have changed the values for the probability of each event to occur. Also, we didn't have the time to implement a system where their actions will be influenced by an emotional state attached like coward, brave,... 
Moreover, the script responsible for the Squad Behaviour (Squadcontroller.cs) acts as an AI director. Further separation would be best practice.
