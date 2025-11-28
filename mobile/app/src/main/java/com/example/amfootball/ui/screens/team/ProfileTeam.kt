package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.dtos.support.PitchInfo
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.lists.ProfilesImageString
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.team.ProfileTeamViewModel
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst

/**
 * Ecrã principal (Stateful) que exibe o perfil detalhado de uma equipa.
 *
 * Este Composable atua como o "Container" ou "Controller". Ele não desenha a UI diretamente,
 * mas é responsável por obter os dados e gerir o estado através do ViewModel.
 *
 * @param viewModel O ViewModel injetado pelo Hilt que fornece os dados da equipa e gere chamadas de API.
 */
@Composable
fun ProfileTeamScreen(
    viewModel: ProfileTeamViewModel = hiltViewModel()
) {
    val profileTeam = viewModel.uiInfoTeam.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    ContentProfileTeam(
        profileInfo = profileTeam.value,
        uiState = uiState,
        retry = { viewModel.retry() },
        modifier = Modifier.padding(all = 16.dp)
    )
}

/**
 * Conteúdo visual (Stateless) do perfil da equipa.
 *
 * Este componente é "burro" (dumb component): não sabe de onde vêm os dados,
 * apenas sabe desenhá-los. Isso torna-o ideal para Previews e Testes.
 *
 * @param profileInfo O objeto de transferência de dados (DTO) com as informações da equipa.
 * @param uiState O estado atual da interface (se está a carregar ou se deu erro).
 * @param retry Função callback a ser executada se o utilizador tentar recarregar após erro.
 * @param modifier Modificadores de layout para personalização externa.
 */
@Composable
private fun ContentProfileTeam(
    profileInfo: ProfileTeamDto,
    uiState: UiState,
    retry: () -> Unit,
    modifier: Modifier = Modifier
) {
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry= { retry() },
        content = {
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
    )
}

/**
 * Sub-componente responsável por renderizar os campos de texto e imagem da equipa.
 *
 * Isola a lógica de apresentação dos campos individuais para manter o [ContentProfileTeam] limpo.
 *
 * @param profileInfo Os dados completos da equipa a serem apresentados nos campos.
 */
@Composable
private fun FieldProfileTeam(
    profileInfo: ProfileTeamDto
) {
    ProfilesImageString(
        image = profileInfo.logo,
        contentDescription =  stringResource(id= R.string.description_logo_team, profileInfo.name),
        modifier = Modifier.fillMaxWidth()
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_team),
        value = profileInfo.name,
        minLenght = TeamConst.MIN_NAME_LENGTH,
        maxLenght = TeamConst.MAX_NAME_LENGTH,
        isSingleLine = true,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_description_team),
        value = profileInfo.description,
        maxLenght = TeamConst.MAX_DESCRIPTION_LENGTH,
        isSingleLine = false,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_filed_foundation_date),
        value = profileInfo.foundationDate.toString(),
        isSingleLine = true,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = "Rank",
        value = profileInfo.rank,
        isSingleLine = true,
        isReadOnly = true,
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_total_points),
        value = profileInfo.totalPoints.toString(),
        isSingleLine = true,
        isReadOnly = true,
    )

    Text(text = stringResource(id = R.string.label_fields_pitch))
    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_pitch_team),
        value = profileInfo.pitch.name,
        minLenght = PitchConst.MIN_NAME_LENGTH,
        maxLenght = PitchConst.MAX_NAME_LENGTH,
        isSingleLine = true,
        isReadOnly = true,
    )

    /* Trocar isto pelo mapa com a morada
    * TextFieldOutline(
        label = stringResource(id = R.string.label_field_address_pitch_team),
        value = profileInfo.pitch.addressPitch,
        minLenght = GeneralConst.MIN_ADDRESS_LENGTH,
        maxLenght = GeneralConst.MAX_ADDRESS_LENGTH,
        isSingleLine = true,
        isReadOnly = true,
    )
    * */
}

@Preview(
    name = "Perfil da Equipa - Sucesso (PT)",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "Team Profile - Success (EN)",
    locale = "en",
    showBackground = true
)
@Composable
fun ProfileTeamScreenPreview() {
    val dummyPitch = PitchInfo(
        name = "Estádio D. Afonso Henriques",
        address = "Guimarães",
    )

    val dummyTeam = ProfileTeamDto(
        id = "1",
        name = "Vitória SC",
        description = "O Conquistador. Clube histórico de Portugal.",
        foundationDate = "1922",
        rank = "Ouro",
        totalPoints = 350,
        logo = "",
        pitch = dummyPitch,
    )

    AMFootballTheme {
        ContentProfileTeam(
            profileInfo = dummyTeam,
            uiState = UiState(isLoading = false, errorMessage = null),
            retry = {},
            modifier = Modifier.padding(16.dp)
        )
    }
}