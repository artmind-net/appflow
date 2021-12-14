# AppFlow registration 
The simplest way to create .NET app flow is using the 'IHostBuilder' extension method 'ArtMind.AppFlow.RegisterAppFlow.

# todo
1. [ ] to do add task name
1. [ ] create uniform logger
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
   - [x] AppTraceTask 
   - [x] AppSafeTask
   - [ ] Async tasks
1. [x] **Flow registragion**
   - [x] Exposing Microsoft.Extensions.Configuration.IConfiguration interface
1. [ ] **Multithreading** - runing multiple services/apps
1. [ ] IContext as TIn TOut in the chaning of tasks
1. [ ] Application lifecycle
1. [ ] Fixes
   1. [x] Application recurrence should continue even if an error occurs.
