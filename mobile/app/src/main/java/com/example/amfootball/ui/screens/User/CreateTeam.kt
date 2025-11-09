package com.example.amfootball.ui.screens.User

import android.net.Uri
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.GroupAdd
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.constants.AddressContants
import com.example.amfootball.data.constants.PitchConstants
import com.example.amfootball.data.constants.TeamConstants
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.ui.components.BackTopBar
import com.example.amfootball.ui.components.InputFields.CreateTextFieldOutline
import com.example.amfootball.ui.components.InputFields.ImagePicker

@Composable
fun CreateTeamScreen(navHostController: NavHostController) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = stringResource(id = R.string.title_page_create_team)
            )},
        content = { paddingValues ->
            ContentCreateTeam(
                modifier = Modifier
                    .padding(paddingValues)
                    .padding(16.dp),
                navHostController = navHostController
            )
        }
    )
}

@Composable
private fun ContentCreateTeam(modifier: Modifier = Modifier,
                              navHostController: NavHostController) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        FieldsCreateTeam(navHostController = navHostController)
    }
}

//Falta criar campo para afixar imagem
@Composable
private fun FieldsCreateTeam(navHostController: NavHostController) {
    var name by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    var imageTeamUri by remember { mutableStateOf<Uri?>(null) }
    var pitchName by remember { mutableStateOf("") }
    var pitchAddress by remember { mutableStateOf("") }

    //Estado de controlo de clicar no botão
    var hasAttemptedSubmit by remember { mutableStateOf(false) }

    //Validações de tentativa de criar com dados obrigatorios vazios
    val isNameBlank = hasAttemptedSubmit && name.isBlank()
    val isPitchNameBlank = hasAttemptedSubmit && pitchName.isBlank()
    val isPitchAddressBlank = hasAttemptedSubmit && pitchAddress.isBlank()

    ImagePicker(
        imageSelectedUri = imageTeamUri,
        onImageSelected = { novoUri ->
            imageTeamUri = novoUri
        },
        modifier = Modifier.padding(bottom = 24.dp)
    )

    CreateTextFieldOutline(
        label = stringResource(id = R.string.label_field_name_team),
        value = name,
        onValueChange = {newName ->
            name = newName
        },
        isSingleLine = true,
        isReadOnly = false,
        isRequired = true,
        isError = isNameBlank
    )

    CreateTextFieldOutline(
        label = stringResource(id = R.string.label_field_description_team),
        value = description,
        onValueChange = {newDescription ->
            description = newDescription
        },
        isSingleLine = false,
        isReadOnly = false,
        isRequired = false,
    )


    Text(text = "Campo da equipa")
    CreateTextFieldOutline(
        label = stringResource(id = R.string.label_field_name_pitch_team),
        value = pitchName,
        onValueChange = {newPitchName ->
            pitchName = newPitchName
        },
        isSingleLine = true,
        isReadOnly = false,
        isRequired = true,
        isError = isPitchNameBlank
    )

    CreateTextFieldOutline(
        label = stringResource(id = R.string.label_field_address_pitch_team),
        value = pitchAddress,
        onValueChange = { newPitchAddress ->
            pitchAddress = newPitchAddress
        },
        isSingleLine = true,
        isReadOnly = false,
        isRequired = true,
        isError = isPitchAddressBlank
    )

    val isFormValid = remember(name, description, pitchName, pitchAddress) {
        val isValidName = name.isNotBlank() && name.length >= TeamConstants.MinNameLength && name.length <= TeamConstants.MaxNameLength
        val isValidDescription = description.isBlank() || (description.isNotBlank() && description.length <= TeamConstants.MaxDescriptionLength)
        val isValidPitchName = pitchName.isNotBlank() && pitchName.length >= PitchConstants.MinNameLength && pitchName.length <= PitchConstants.MaxNameLength
        val isValidPitchAddress = pitchAddress.isNotBlank() && pitchAddress.length >= AddressContants.MinAddressLength && pitchAddress.length <= AddressContants.MaxAddressLength

        isValidName && isValidDescription && isValidPitchName && isValidPitchAddress
    }

    Button(
            onClick = {
                hasAttemptedSubmit = true

                if (isFormValid) {
                    navHostController.navigate(RoutesNavBarTeam.HOME_PAGE_TEAM) {
                        popUpTo(0)
                    }
                }
            },

    modifier = Modifier
        .fillMaxWidth()
        .height(56.dp)
    ) {
        Icon(imageVector = Icons.Default.GroupAdd, contentDescription = "Enviar Convite")
        Spacer(Modifier.width(8.dp))
        Text(text = stringResource(id = R.string.button_create_team))
    }
}

@Preview(name = "Criar Equipa - En",
    locale = "en",
    showBackground = true)
@Preview(name = "Criar Equipa - PT",
    locale = "pt-rPT",
    showBackground = true)
@Composable
fun PreviewCreateTeamScreen() {
    CreateTeamScreen(navHostController = rememberNavController())
}