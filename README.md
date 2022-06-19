# EcsLite EventBus

💡 EventBus is an extension for [LeoECSCommunity/ecslite](https://github.com/LeoECSCommunity/ecslite) that simplifies exporting state data from the ECS core into the observing views that handle presenting state to the user.  EventBus makes writing performant, well-structured code easier.

⚠️ This extension is NOT intended for communication between ECS systems.  That should be done with the built-in ecs-lite mechanisms such as flag components and filters.  Rather, this extension is focused on exporting state to outside observers.

🧩 This repository has its own [extended version](https://github.com/RealityStop/ecslite-EventBusExtended.git), which requires the ecs-lite [ServiceContainer](https://github.com/RealityStop/ecslite-ServiceContainer) extension.  If you're using ServiceContainer, there are a few extra features in the EventBusExtended that add to the base set of features.



# Getting Started

## General
Simply download the source repository and include the .cs files as you normally would.

## Unity 

EventBus can either be imported via the Unity Package Manager or used directly from source.

Open the Unity Package Manager, and click the "+" button in the top-left corner :

![](https://imgur.com/v92tiFD.png)

and add the following url:
> https://github.com/RealityStop/ecslite-EventBus.git

(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)

###  Manual Unity install:
Alternatively, open Packages/manifest.json and add this line under dependencies:

	"dev.leoecscommunity.ecslite.eventbus": "https://github.com/RealityStop/ecslite-EventBus.git"
	
(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)
	
## Namespace
All ServiceContainer code is under the 
```
Leopotam.EcsLite
```
namespace to simplify integration with other ecslite extensions.


## Event Types
To give our example some context, we need to cover the four event types which will frame how we approach this problem.  There are four core event types split into two categories: *universal*, and *entity-scoped*.  *Universal* events are events that operate on the ECS scope, whereas *entity-scoped* events are tied to a particular entity.   

| Universal | Entity-Scoped|
|--|--|
| Unique Events | Entity Events |
| Global Events | Flag Component Events |

#### Universal Event Types
**Unique Events** are universally accessible events, and as such, do not require an entity to raise the event or observe.   These are well-suited to game-state changes, such as the user pausing, or the player character dying. Because Unique Events are unique, attempting to raise a unique event that has already been created will return the existing instance.  For instance, if the player is taking damage from multiple sources and an event to signify the game state change has already been raised earlier  any attempt to raise the event again will return the existing event.  Once the listeners are notified and the event is "consumed" it will be deleted and could be created again.  This makes it simpler to code against, as your code can treat any event as though it were new.  

**Global Events** are essentially the same concept as a Unique Event, except that multiple of them can be raised at a time.  These are useful for events that still happen at a universal level, but aren't unique.  Perhaps we display a user score in the user interface, and we want to display a text popup whenever the score changes, but our game design allows multiple sources for score events (pickups, killing enemies, time passing, and so on).  These are all events that could raise a ScoreChange event, and could potentially happen in the same frame, so we implement that as a Global Event in order to display text for each such event to the user.

### Entity-Scoped Event Types
**Flag Events** are events that are raised by the presence of the flag component on an entity.  It doesn't matter how the flag component got there: either adding the flag component using the Event Bus or through a regular pool `.Add()` will still cause events to be raised if there are listeners.  Flag Components typically carry no data, and the pools exist on the same ECSWorld as the entity.

**Entity Events** are data-carrying events that are attached to the Entity and multiple can be raised at a time.  This can be useful for things like collisions, where the data about the collision is important, and we can't use components on the entities, since multiple might occur at once.  While it would be possible to track the collision data in a collection in the component, we'd need an *additional* component to signal when the collection is non-empty, and the observers would each have to unpack and collect the appropriate data.  Using Entity Events abstracts that away by storing the event data in a separate world (to prevent memory being reserved for each event type in your main worlds) and delivering the data directly to the observers.

> Note: due to the optimizations required to store multiple entity events in a performant manner, it is not possible to manually check for or fetch Entity Events.  The Event Bus will notify listeners at the correct time, but it does not off the ability to query for the presence of them.




# A compelling example

For this example, we're going to imagine a case where we have a Unity project in which we've already programmed ECS systems to construct many game units, move them around and have them attack each other.  Additionally, we've added a system to construct Unity GameObjects to render the on-screen units. However, we need to show a healthbar for injured units as they lose health.

## The problem:
Our GameObjects need to observe the state of the ECS game so they can update and render healthbars that reflect a unit's health.  Without the EventBus, the GameObject has no choice but to pool every frame for health (or for a flag component that indicates a change).  This has a large performance hit, as every frame the game has to check and update.  The vast majority of these updates are wasted, as nothing has changed.   Additionally, this approach is more error prone as, unless the developer manually sets the order, observer scripts might run before or after the ECS systems.  Lastly, the GameObjects have a fairly strong dependency on the inner workings of the game systems.

![test](../assets/images/defaultGameLoop.png)

Event Bus attempts to address each of these issues.

## Using EventBus to solve the problem.
EventBus operates by having observers subscribe to events and only execute observer logic when the event happens.  EventBus updates subscribers at a predictable time in your ECS cycle.  This results in much more performant, deterministic, and flexible code.


![test](../assets/images/eventBusGameLoop.png)

🎓Unity users have additional helpers in the [extended version](https://github.com/RealityStop/ecslite-EventBusExtended.git).




### Ecs Startup

EventBus is designed to be easy to use, even on existing projects.  EventBus is a regular C# class that offers a single place for registering events and consuming them, using it's own event world to reduce memory pressure on your own entities.  Additionally, it operates on the principle of *delayed reactivity*, by recording them as they are raised and executing them at a later point in time, ensuring that the ECS-systems remain deterministic even when observed by outside scripts.  To succeed at this, EventBus is tied into the ECS systems queue.


> 🎓 NOTE: All examples assume the use of [ServiceContainer](https://github.com/RealityStop/ecslite-ServiceContainer), which allows us to abstract away the construction specifics and focus on the EventBus.

```C#
void Start()  
{ 
  //We can either construct the EventBus here, or (in Unity) have it as a hosted service so that  
  //world observers can set up listeners before our ECS systems have spun up. 
  _eventBus = ServiceContainer.Get<IEventBus>();
  
  _systems = new EcsSystems (new EcsWorld (), ServiceContainer.GetCurrentContainer());  
  _systems
     //Add other worlds
     .AddWorld(_eventBus)	//we add the eventBus world.  An extension method simplifies writing this.

  //add other systems...

  //Add handler for specific event type, if you need early processing or an enforced order  
  .Add(_eventBus.EntityEvents.ProcessorFor<HealthChangedEvent>())  
	  
  //any other systems...
	  
  //Process all the remaining events.  This should be among the very last things in the queue.
  .AddAllEvents(_eventBus)
  .Inject()
  .Init()
```

### Creating an event component
To create an event, we create a new component and tag it with one of the event types.  For most event types, these interfaces don't add any requirements, they are just flags to help the EventBus to understand what to do with each without relying on reflection.  The one exception is IEntityEvent, which is an event that inherently references a particular entity, and so it requires a property to store the entity reference the event is being raised for.  Again, more detail on the specifics of each Event type later.

```c#
public struct HealthChangedEvent : IEventEntity  
{  
	//Entity Events reference a specific entity, and we need a place to store that.  Since C#  
	//interfaces don't allow fields, we have to use a property here.  
	public EcsPackedEntityWithWorld Source { get; set; }  
	  
	//But regular data can be fields.
	public float newHealth;  
	public float newHealthPercent;
}
```

This event can now be raised and consumed by our other scripts.

### Raising an Event

```C#
public class UpdateDamagedUnits : IEcsRunSystem  
{  
	private readonly EcsServiceInject<IEventBus> _eventBus;  
	//  filters, pools etc.
	
	public void Run(EcsSystems systems)  
	 { 
		foreach (var healthEntity in _filter)
		{
			ref var healthComponent = ref _healthPool.Get(healthEntity);

			//Asking the EventBus to add an event is just like adding to a pool, with the exception that Entity Events take the packed entity.
			ref var changedEvent = ref _eventBus.Value.EntityEvents.Add<HealthChangedEvent>(_world.PackEntityWithWorld(positionEntity));  
			
			//Because the eventBus uses delayed notification,
			//we can take our time and set up the event, which
			//is just like any other component.
			changedEvent.newHealth = healthComponent.CurrentHealth;   
		 }		
	 }
 }
```

And with that, we have a system adding events to the Event Bus.  There's no reason this has to be a dedicated system.  When events are created and when they are executed are under your control, so you can have one system adding them or twelve, the Event Bus doesn't care.

But it isn't enough to just create events, we also observe them, so let's dive into that next.

### Consuming Events
Observing events can be done two ways: either **ListenTo** or **SubscribeTo**, which are identical in the parameters you pass them.  Where they differ is how they handle *terminating* the observation.  **ListenTo** requires the developer handle manually stopping the observation, while **SubscribeTo** returns an IDisposable that will clean up the subscription, which can be stored and disposed in bulk. 

If you're writing single-purpose scripts, using **ListenTo** will be slightly higher performing.  If you are bulk subscribing, then **SubscribeTo** can be more convenient, but does come with a bit of reserved memory for the subscription tracking.  However, this memory is reused between subscriptions, so ultimately use the one you are most comfortable with.

> Note: The extended version comes with a reusable disposable container that makes bulk storage and cleaning easy to use.  Check there for a demonstration of the SubscribeTo

Let's take a look at what this might look like for a simple single-purpose ListenTo:

```C#

  //Observing:
  //Entity events take the packed entity, and a callback to call when the event triggers.
  _eventBus.EntityEvents.ListenTo<HealthChangedEvent>(PackedEntity, OnHealthChanged);  
 
  private void OnHealthChanged(EcsPackedEntityWithWorld packed, ref HealthChangedEvent item)  
  {  
    // update the healthbar.  The event instance data is passed as a parameter, and can be directly used.
  }

  //Terminating:
  //Since we did a ListenTo, at some point, we have to call RemoveListener to free the memory in the EventBus.
  _eventBus.EntityEvents.RemoveListener<HealthChangedEvent>(PackedEntity, OnHealthChanged);  
```

## Performance
All of this is meaningless if the performance of the Event Bus is too low to use.  So I tested the performance on my aging 2015 Skylake processor in Unity.  All of the following consist of the same game systems, the tests only really manipulate the view (Unity gameobject) side to adjust how those views are accessing the ECS data.  The only logic changing on the ECS side is adjusting how we're raising the events to match the changes on the View side (so if we swap the view GameObjects from Entity Events to Flag Events, we swap how we raise them as well in the ECS systems).  The important part is that all of the logic for moving the entities and damaging them is always happening, event when there's nothing listening.

Each test spawns entities around the play area (screen) that are moving with constant velocity and bouncing off the edge of the screen. Anytime they hit an edge, their health is reduced (though they never die, to eliminate variance).  GameObject views are wired to observe the entities and reflect the ECS state.

Entities spawn until my computer drops the 2-second moving average fps below 60 (they stop spawning entirely at 59 fps to prevent overrun during those two seconds).

### Test #1 - Max Entities 
> Tests how many entities the processor can handle with all of the moving and damaging code.  No unity game objects, just entities, which helps to learn just how much the act of creating GameObjects costs, and preps all of the Worlds and Pools so we aren't allocating memory in the later tests.

**Result** -> 489,217  entites created above 60fps

### Test #2 - Max GameObject Views
> Tests how GameObjects the processor can handle.   The GameObjects grab their initial position, but don't update in any way. It's just a test of how much "tax" creating the gameobjects inflicts. Also serves to prep my GameObject pools.

**Result** -> 155,495  entities created above 60fps

### Test #3 - Polling Position
> Flips on Monobehaviors that sync position from the ECS world in Update(). As you can see, just updating position and nothing more reduces performance by ~80%. 
This is the 'naive' approach that many would opt for and a solution isn't immediately apparent, because the position is changing each frame and the GameObjects need to reflect that.  But or next test shows just how much we can improve this.

**Result** -> 32,412 entities created above 60fps

### Test #4 - Optimized Update
> We have a trick up our sleeve. We use a unique event to iterate through all the views and tell each that it's time to update. Because we're staying in C#-land we don't incur the marshalling penalty Unity incurs when **IT** iterates through the objects and we double the number of objects we can support. Importantly, this subscription is driven by the views, so our ECS systems are completely blind and have no idea that things need to be updated.  From here on out, we'll use this approach for position.

**Result** -> 74,357 entities created above 60fps

### Test #5 - Polling Health
> Still using the Optimized Update for position, here we have added a new polling update, this time for health that is NOT changing each frame, which is even more egregious. However, it is included here as a measure of the impact the `Optimized Updates` methodology has. In `Polling Position` we had 1 polling monobehavior per GameObject. Here we again have 1 polling monobehavior (for health this time) in addition to the approach built in Optimized Update.  In this way, we get a more concrete measurement of the total cost of updating the position of every entity every frame via the Optimized Update. It comes out to 704 entities. Not bad, and once we get rid of the polling on health, we're only going to go up from here.

**Result** -> 31,708 entities created above 60fps

### Test #6  - Sporadic Health Events
> Of course, we don't need to check health each frame. It only changes when they hit the edge. So, we listen for that event and drop the whole polling thing for a huge boost.

**Result** -> 71,064 entities created above 60fps

### Test #7 - Sporadic Flag Component Events
> The EventBus also supports doing the same thing with flag components, meaning your systems don't even have to know about events! Performance is roughly the same.

**Result** -> 70,854 entities created above 60fps



## License
All code in this repository is covered by the [Mozilla Public License](https://www.mozilla.org/en-US/MPL/), which states that you are free to use the code for any purpose, commercial or otherwise, for any type of application or purpose, and that you are free to release your works under whatever license you choose.  However, regardless of application or method, this code remains under the MPL license, and all modifications or portions of it must also remain under the MPL license and be made available, but this is limited to the covered code and modifications to it.  It is NOT viral, nor does it enforce the MPL license on any other portion of your code, as in strong copyleft licenses like GPL and its derivatives.   The intent is that this code is MPL, shall always be MPL regardless of author, and that it and all modified versions should be public and available to all.

Simple guidelines:
| Use| Modify |
|--|--|
| Put a text file in your distribution that states OSS usage, with a link to this repository among any others. | Same as **Use** and make modifications public under the MPL by either issuing a pull request to this repository, forking it, or hosting your own. |

However, these are only guidelines, please see the actual license and [Additional license FAQ](https://www.mozilla.org/en-US/MPL/2.0/FAQ/) for actual terms and conditions.
