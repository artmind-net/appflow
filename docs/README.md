# AppFlow registration 
The simplest way to create .NET app flow is using the 'IHostBuilder' extension method 'ArtMind.AppFlow.RegisterAppFlow.

# ToDo
1. [x] **Service Options**
   - [x] _Occurrence_ The number of service flow cycles to be executed
   - [x] _Recurrence_ The minimum time interval duration of a service flow cycle, until to start next session.
   - [x] _Scheduler_ Schedule service to start at
1. [x] **App Options**
   - [x] _Postpone_ Postpone application start. Flow will start after the postpone interval will elapse
   - [x] _Scheduler_ Schedule cycles
1. [ ] **Flow Extentions**
   - [x] if/else brach
   - [ ] Nested workers / middlewares (inception) a->a1'->a''->....
1. [ ] **AppTasks**
   - [x] TraceTask 
   - [x] SafeTask
   - [x] FlowTasks - a chain of tasks where the out type of the previous task became input type for the following task. Task<T,TResult>
   - [ ] Async tasks
   - [ ] ~Name tasks (like named injection in lemar)~
1. [x] **Flow registragion**
   - [x] Exposing Microsoft.Extensions.Configuration.IConfiguration interface
1. [ ] **Multithreading** - runing multiple services/apps
1. [x] Application lifecycle (.net)
1. [ ] ~Uniform logger~
1. [x] Fixes
   1. [x] Application recurrence should continue even if an error occurs.


   - sample console
		* scenario 1: Text to speach:
			* W1: Setup (say welcome, chouse instaled voice, save results)
			* W2: ConsoleHandler (read console input)
			* W3: Text to speach lib
			* W4: Closure, save data, say goodby.
		* scenario 2: ceva cu if 
		* scenario 3: ceva cu while
   - sample service
		* scenario 1: ETL
		* scenario 2: ForgetMe (BUS event) (un if ceva)
		* scenario 3: ceva cu while