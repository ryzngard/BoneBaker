using BoneBaker.Data;

using Microsoft.AspNetCore.Components.Forms;

namespace BoneBaker.Pages
{
  public partial class CreateCharacter
  {
    private EditContext? editContext;
    private Character Character { get; set; } = new();
    private ValidationMessageStore? messageStore;

    // UI state for adding training and knowledge
    private string newTrainingName = string.Empty;
    private Die newTrainingDie = Die.D4;
    private string? newTrainingError;

    private string newKnowledgeName = string.Empty;
    private Die newKnowledgeDie = Die.D4;
    private string? newKnowledgeError;

    protected override void OnInitialized()
    {
      editContext = new(Character);
      editContext.OnFieldChanged += OnFieldChanged;
      editContext.OnValidationRequested += OnValidationRequested;
      messageStore = new(editContext);

      base.OnInitialized();
    }

        private (bool IsValid, List<Die> DuplicateDies) ValidateStyleInternal()
        {
            messageStore.NotNull();
            messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Style)));
            // clear per-field style messages
            messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Style.Tough)));
            messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Style.Quick)));
            messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Style.Cool)));
            messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Style.Planner)));
            messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Style.Techie)));

            var stats = Character.Style.AsDieStats();
            var duplicateGroups = stats.GroupBy(s => s.Die).Where(g => g.Count() > 1).ToList();
            var dupDies = duplicateGroups.Select(g => g.Key).ToList();
            if (duplicateGroups.Any())
            {
                foreach (var die in dupDies)
                {
                    if (Character.Style.Tough == die) messageStore.Add(() => Character.Style.Tough, "Duplicate die used in style");
                    if (Character.Style.Quick == die) messageStore.Add(() => Character.Style.Quick, "Duplicate die used in style");
                    if (Character.Style.Cool == die) messageStore.Add(() => Character.Style.Cool, "Duplicate die used in style");
                    if (Character.Style.Planner == die) messageStore.Add(() => Character.Style.Planner, "Duplicate die used in style");
                    if (Character.Style.Techie == die) messageStore.Add(() => Character.Style.Techie, "Duplicate die used in style");
                }

                messageStore.Add(() => Character.Style, "Duplicate die values found in style");
            }

            return (!duplicateGroups.Any(), dupDies);
        }

    private (bool IsValid, bool HasEmptyAdditionalNames) ValidateTraining()
    {
      messageStore.NotNull();
      // clear any existing training-related messages
      messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Training)));
      foreach (var field in new[] { nameof(Character.Training.Awareness), nameof(Character.Training.Combat), nameof(Character.Training.Education), nameof(Character.Training.Fitness), nameof(Character.Training.Hardware), nameof(Character.Training.Medicine), nameof(Character.Training.Social), nameof(Character.Training.Software), nameof(Character.Training.Subtlety), nameof(Character.Training.Vehicles) })
      {
        messageStore.Clear(new FieldIdentifier(Character, field));
      }

      bool hasEmptyAdditional = Character.Training.AdditionalStats.Any(s => string.IsNullOrWhiteSpace(s.Name));
      if (hasEmptyAdditional)
      {
        messageStore.Add(() => Character.Training, "Additional training stat names are required");
      }




      bool isValid = !hasEmptyAdditional;
      return (isValid, hasEmptyAdditional);
    }

    private (bool IsValid, List<Die> DuplicateDies, bool HasEmptyNames) ValidateKnowledge()
    {
      messageStore.NotNull();
      messageStore.Clear(new FieldIdentifier(Character, nameof(Character.Knowledge)));
      bool hasEmpty = Character.Knowledge.Stats.Any(s => string.IsNullOrWhiteSpace(s.Name));
      if (hasEmpty)
      {
        messageStore.Add(() => Character.Knowledge, "Knowledge stat names are required");
      }
      var duplicateGroups = Character.Knowledge.Stats.GroupBy(s => s.Die).Where(g => g.Count() > 1).ToList();
      var duplicateDies = duplicateGroups.Select(g => g.Key).ToList();
      if (duplicateGroups.Any())
      {
        messageStore.Add(() => Character.Knowledge, "Duplicate die values in knowledge");
      }

      return (!hasEmpty && !duplicateGroups.Any(), duplicateDies, hasEmpty);
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
      messageStore.NotNull();
      messageStore.Clear();
      ValidateName();
      ValidateGender();
      ValidateStyleInternal();
      ValidateTraining();
      ValidateKnowledge();
    }

    private void ValidateName()
    {
      messageStore.NotNull();
      if (Character.Name.IsNullOrWhitespace())
      {
        messageStore.Add(() => Character.Name, "Name is required");
      }
    }

    private void ValidateGender()
    {
      messageStore.NotNull();
      if (Character.Gender.IsNullOrWhitespace())
      {
        messageStore.Add(() => Character.Gender, "Gender is required");
      }
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
      messageStore.NotNull();

      messageStore.Clear(e.FieldIdentifier);

      switch (e.FieldIdentifier.FieldName)
      {
        case nameof(Character.Name):
          ValidateName();
          break;
        case nameof(Character.Gender):
          ValidateGender();
          break;
        case nameof(Character.Style):
          ValidateStyleInternal();
          break;
        case nameof(Character.Knowledge):
          ValidateKnowledge();
          break;
        case nameof(Character.Training):
          ValidateTraining();
          break;
      }

      // If any training field changed, re-run full validation so per-field messages update
      if (e.FieldIdentifier.FieldName.StartsWith(nameof(Character.Training) + ".")
          || e.FieldIdentifier.FieldName == nameof(Character.Training)
          || e.FieldIdentifier.FieldName.StartsWith(nameof(Character.Style) + ".")
          || e.FieldIdentifier.FieldName == nameof(Character.Style))
      {
        // Clear all messages and revalidate to map messages to specific fields
        messageStore.Clear();
        // Re-run specific validators so style-related validation uses ValidateStyleInternal
        ValidateName();
        ValidateGender();
        ValidateStyleInternal();
        ValidateTraining();
        ValidateKnowledge();
      }
    }

    private async Task SaveCharacter()
    {
      var characters = await StorageService.GetCharactersAsync();
      var allCharacters = characters.Append(Character).ToArray();
      await StorageService.SetCharactersAsync(allCharacters);
      Navigation.NavigateTo($"/character/{Character.Identifier}");
    }

    private void AddTrainingStat()
    {
      newTrainingError = null;
      var stat = new DieStat(newTrainingName?.Trim() ?? string.Empty, newTrainingDie);
      if (!Character.Training.TryAddStat(stat, out var err))
      {
        newTrainingError = err;
        return;
      }

      // After adding, run page-level validation to map any duplicates to fields
      messageStore.Clear();
      ValidateTraining();

      // clear inputs
      newTrainingName = string.Empty;
      newTrainingDie = Die.D4;
      StateHasChanged();
    }

    private void AddKnowledgeStat()
    {
      newKnowledgeError = null;
      var stat = new DieStat(newKnowledgeName?.Trim() ?? string.Empty, newKnowledgeDie);
      if (!Character.Knowledge.TryAddStat(stat, out var err))
      {
        newKnowledgeError = err;
        return;
      }

      messageStore.Clear();
      ValidateKnowledge();

      newKnowledgeName = string.Empty;
      newKnowledgeDie = Die.D4;
      StateHasChanged();
    }

    private void RemoveTrainingAdditional(DieStat stat)
    {
      Character.Training.AdditionalStats = Character.Training.AdditionalStats.Remove(stat);
      // revalidate via page-level validation
      messageStore?.Clear();
      ValidateTraining();
      StateHasChanged();
    }

    private void RemoveKnowledgeStat(DieStat stat)
    {
      Character.Knowledge.Stats = Character.Knowledge.Stats.Remove(stat);
      messageStore?.Clear();
      ValidateKnowledge();
      StateHasChanged();
    }

    private void Cancel()
    {
      Navigation.NavigateTo("/");
    }
  }
}