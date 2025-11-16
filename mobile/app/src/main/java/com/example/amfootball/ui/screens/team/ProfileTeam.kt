package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.team.ProfileTeamInfoDto
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.lists.ProfilesImage
import com.example.amfootball.ui.viewModel.team.ProfileTeamViewModel

@Composable
fun ProfileTeamScreen(
    navHostController: NavHostController,
    idTeam: String,
    viewModel: ProfileTeamViewModel = viewModel()
) {
    val profileTeam = viewModel.uiInfoTeam.observeAsState(initial = ProfileTeamInfoDto.profileExempleTeam())

    //TODO: Meter para aparecer mais coisas
    ContentProfileTeam(
        profileInfo = profileTeam.value,
        modifier = Modifier.padding(all = 16.dp),
    )
}


@Composable
private fun ContentProfileTeam(
    profileInfo: ProfileTeamInfoDto,
    modifier: Modifier = Modifier) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        FieldProfileTeam(
            profileInfo = profileInfo
        )
    }
}

/**
 * Metodo que desenha os dados do profile
 * */
@Composable
private fun FieldProfileTeam(
    profileInfo: ProfileTeamInfoDto
) {
    ProfilesImage(
        image = profileInfo.logo,
        contentDescription =  stringResource(id= R.string.description_logo_team, profileInfo.name),
        modifier = Modifier.fillMaxWidth()
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_team),
        value = profileInfo.name,
        isSingleLine = true,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_description_team),
        value = profileInfo.description,
        isSingleLine = true,
        isReadOnly = true,
    )

    Text(text = stringResource(id = R.string.label_fields_pitch))
    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_pitch_team),
        value = profileInfo.pitch.namePitch,
        isSingleLine = true,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_address_pitch_team),
        value = profileInfo.pitch.addressPitch,
        isSingleLine = true,
        isReadOnly = true,
    )
}

@Preview(
    name = "Página de Perfil da Equipa - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "Perfil Page Team - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun ProfileTeamScreenPreview() {
//Depois meter Id valido, de exemplo ou da BD
    ProfileTeamScreen(
        idTeam = "2",
        navHostController = rememberNavController()
    )
}