package com.example.amfootball.data.actions.forms

import android.net.Uri

data class FormTeamActions(
    val onNameChange: (String) -> Unit,
    val onDescriptionChange: (String?) -> Unit,
    val onImageChange: (Uri?) -> Unit,
    val onNamePitchChange: (String) -> Unit,
    val onAddressPitchChange: (String) -> Unit
)