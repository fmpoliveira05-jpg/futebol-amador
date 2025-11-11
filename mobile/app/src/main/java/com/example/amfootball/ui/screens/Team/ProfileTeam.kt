package com.example.amfootball.ui.screens.Team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.ui.components.BackTopBar
import com.example.amfootball.ui.components.InputFields.TextFieldOutline

@Composable
fun ProfileTeamScreen(
    navHostController: NavHostController,
    idTeam: String
) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = stringResource(id = R.string.title_page_profile_team)
            )},
        content = { paddingValues ->
            ContentProfileTeam(
                modifier = Modifier
                    .padding(paddingValues)
                    .padding(all = 16.dp),
            )
        }
    )
}


@Composable
private fun ContentProfileTeam(modifier: Modifier = Modifier) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        FieldProfileTeam()
    }
}

/**
 * Arranjar forma de meter a aparecer a imagem
 * + Lista de equipas
 * */
@Composable
private fun FieldProfileTeam() {
    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_team),
        value = "Equipa xxxx",
        isSingleLine = true,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_description_team),
        value = "Descrição equipa",
        isSingleLine = true,
        isReadOnly = true,
    )

    Text(text = "Campo da equipa")
    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_pitch_team),
        value = "Campo xxxxx",
        isSingleLine = true,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_address_pitch_team),
        value = "Rua xxxx",
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