mergeInto(LibraryManager.library, {

    SaveData: function() {
        psiturk.saveData();
    },

    AddData: function(data) {
        psiturk.recordTrialData([Pointer_stringify(data)]);
    },
    
    EndTask: function() {
        // Questionnaire(psiturk);
        psiturk.completeHIT();
    },

    NoRefresh: function() {
        psiturk.finishInstructions();
    },
});