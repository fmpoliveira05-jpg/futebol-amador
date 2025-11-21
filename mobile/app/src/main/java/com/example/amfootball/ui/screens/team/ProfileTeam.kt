package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.example.amfootball.R
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.lists.ProfilesImageString
import com.example.amfootball.ui.viewModel.team.ProfileTeamViewModel
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst

/**
 * Ecrã principal que exibe o perfil detalhado de uma equipa de futebol.
 *
 * Este Composable atua como o controlador da vista, gerindo a comunicação com o
 * [ProfileTeamViewModel] e os estados de carregamento/erro.
 *
 * Funcionalidades:
 * - Obtém os dados da equipa e o estado da UI do ViewModel.
 * - Utiliza [LoadingPage] para mostrar spinners ou mensagens de erro se necessário.
 * - Renderiza os detalhes da equipa quando o carregamento é concluído com sucesso.
 *
 * @param viewModel O ViewModel injetado pelo Hilt que fornece os dados da equipa.
 */
@Composable
fun ProfileTeamScreen(
    viewModel: ProfileTeamViewModel = hiltViewModel()
) {
    val profileTeam = viewModel.uiInfoTeam.observeAsState(initial = ProfileTeamDto.profileExempleTeam())
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    //TODO: VER SE ESTÃO TODOS OS DADOS
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry= { viewModel.retry() },
        content =  {
            ContentProfileTeam(
                profileInfo = profileTeam.value,
                modifier = Modifier.padding(all = 16.dp)
            )
        }
    )
}

/**
 * Estrutura de layout do conteúdo do perfil.
 * Centra vertical e horizontalmente os campos da equipa.
 *
 * @param profileInfo Os dados da equipa a exibir.
 * @param modifier Modificadores de layout (margens, etc.).
 */
@Composable
private fun ContentProfileTeam(
    profileInfo: ProfileTeamDto,
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
 * Componente que desenha os campos individuais com a informação da equipa.
 * Inclui o logótipo, nome, descrição, data de fundação, rank, pontos e estádio.
 *
 * @param profileInfo O objeto DTO com os dados completos da equipa.
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
    ProfileTeamScreen()
}