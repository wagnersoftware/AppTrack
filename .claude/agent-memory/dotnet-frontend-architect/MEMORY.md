# AppTrack Frontend Architect Memory

## Project Layout (confirmed Mar 2026)
- WPF project: `AppTrack.Wpf/` (csproj: AppTrack.WpfUi.csproj, namespace AppTrack.WpfUi / AppTrack.Wpf)
- Blazor project: `AppTrack.BlazorUi/` (excluded from AppTrack.sln; build independently)
- Frontend models: `Models/` (project: AppTrack.Frontend.Models)
- API service: `ApiService/` (project: AppTrack.Frontend.ApiService)

## WPF Architecture Patterns

### Shared Dialog View
`JobApplicationView` is a single Window used for both Create and Edit. The `IsEditView`
bool on `JobApplicationViewModelBase` controls visibility of edit-only fields (Status,
ApplicationText) via `BoolToVisibilityConverter`. `WindowService` maps both
`CreateJobApplicationViewModel` and `EditJobApplicationViewModel` to `JobApplicationView`.

### WindowService Pattern
`WindowService.ShowWindow<TViewModel>` uses a switch expression to map ViewModel types
to Window types. Dialogs are shown via `window.ShowDialog()`. Returns `bool?` (true =
saved, false/null = cancelled).

### ViewModel Construction for Edit
Edit ViewModels that require a runtime-provided model use `ActivatorUtilities.CreateInstance`
(not DI container directly) because the model is only available at call time:
```csharp
var vm = ActivatorUtilities.CreateInstance<EditJobApplicationViewModel>(_serviceProvider, model);
```

### Form ViewModel Base
`AppTrackFormViewModelBase<T>` provides `SaveCommand`, `CancelCommand`, `ResetErrorsCommand`,
and `Errors` dictionary. `Save` closes the window with `DialogResult = true`.
`ResetErrors(propertyName)` clears a single field's errors on change.

### List Refresh after Edit (WPF)
Replace-by-index pattern (preserves order):
```csharp
int index = JobApplications.IndexOf(model);
JobApplications.RemoveAt(index);
JobApplications.Insert(index, apiResponse.Data!);
```

## Blazor Architecture Patterns

### Dialog Pattern (MudBlazor)
- Dialog component: `.razor` + `.razor.cs` partial class
- Receives existing data via `[Parameter]` on the code-behind class
- `[CascadingParameter] IMudDialogInstance MudDialog` for close/cancel
- Close with result: `MudDialog.Close(DialogResult.Ok(data))`
- Cancel: `MudDialog.Cancel()`
- `IModelValidator<T>` injected directly into dialog component (transient via DI)

### Edit Dialog Copy Pattern
In `OnParametersSet`, copy all fields from the `[Parameter]` model into a local `_model`
instance. This prevents in-place mutation of the list item before the user saves:
```csharp
protected override void OnParametersSet()
{
    _model = new JobApplicationModel { /* copy all fields */ };
    _startDate = JobApplication.StartDate != DateOnly.MinValue
        ? JobApplication.StartDate.ToDateTime(TimeOnly.MinValue) : null;
}
```

### Dialog Parameters (typed)
Use `DialogParameters<TDialog>` with lambda selectors for type safety:
```csharp
var parameters = new DialogParameters<EditJobApplicationDialog>
{
    { x => x.JobApplication, model }
};
```

### List Refresh after Edit (Blazor)
Replace-by-index pattern after dialog closes with updated model:
```csharp
var index = _jobApplications.IndexOf(model);
if (index >= 0) _jobApplications[index] = updatedModel;
await InvokeAsync(StateHasChanged);
```

### Shared DialogOptions
Both create and edit dialogs share the same `DialogOptions` instance
(`BackdropClick = false, MaxWidth = Medium, FullWidth = true`).

## Validation Wiring
- `IModelValidator<T>` is transient; each dialog gets its own instance
- `ModelValidator.Validate(model)` returns false and populates `Errors` dict on failure
- `ModelValidator.ResetErrors(propertyName)` called on each field change handler
- Error display: `Error="@ModelValidator.Errors.ContainsKey(propName)"` and
  `ErrorText="@GetFirstError(propName)"` on MudTextField
- `GetFirstError`: `ModelValidator.Errors.GetValueOrDefault(key)?.FirstOrDefault() ?? string.Empty`

## Blazor Validator Registration Pattern
Validators are registered one-by-one in `AppTrack.BlazorUi/Program.cs` as transient
`IValidator<T>` bindings. The open-generic `IModelValidator<>` → `ModelValidator<>` is
registered after all concrete validators. Adding a new validator requires two lines:
```csharp
builder.Services.AddTransient<IValidator<AiSettingsModel>, AiSettingsModelValidator>();
builder.Services.AddTransient<IValidator<PromptParameterModel>, PromptParameterModelValidator>();
```

## Sub-Dialog (Nested Dialog) Pattern
When a dialog needs to open another dialog (e.g., AiSettingsDialog → PromptParameterDialog),
inject `IDialogService` into the parent dialog's code-behind. Use a separate `_paramDialogOptions`
constant with `BackdropClick = false, MaxWidth = Small` for the child. Call
`DialogService.ShowAsync<ChildDialog>("")` and `await dialog.Result` just like from a page.

## Key File Paths
- WPF MainViewModel: `AppTrack.Wpf/ViewModel/MainViewModel.cs`
- WPF form base: `AppTrack.Wpf/ViewModel/Base/AppTrackFormViewModelBase.cs`
- WPF JobApp view base: `AppTrack.Wpf/ViewModel/Base/JobApplicationViewModelBase.cs`
- WPF shared dialog view: `AppTrack.Wpf/View/JobApplicationView.xaml`
- WPF WindowService: `AppTrack.Wpf/WindowService/WindowService.cs`
- WPF App DI: `AppTrack.Wpf/App.xaml.cs`
- Blazor Home page: `AppTrack.BlazorUi/Components/Pages/Home.razor(.cs)`
- Blazor dialogs: `AppTrack.BlazorUi/Components/Dialogs/`
- Blazor Program.cs: `AppTrack.BlazorUi/Program.cs`
- Blazor layout: `AppTrack.BlazorUi/Components/Layout/MainLayout.razor(.cs)`
- JobApplicationModel: `Models/JobApplicationModel.cs`
- AiSettingsModel: `Models/AiSettingsModel.cs`
- PromptParameterModel: `Models/PromptParameterModel.cs`
- ModelBase: `Models/Base/ModelBase.cs` (Id, CreationDate, ModifiedDate)
