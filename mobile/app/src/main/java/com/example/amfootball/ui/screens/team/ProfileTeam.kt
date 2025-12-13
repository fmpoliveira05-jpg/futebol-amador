package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
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
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.core.utils.PitchConst
import com.example.amfootball.core.utils.TeamConst
import com.example.amfootball.data.events.UiState
import com.example.amfootball.data.remote.dtos.team.ProfileTeamDto
import com.example.amfootball.domains.enums.UserRole
import com.example.amfootball.ui.actions.profiles.ProfileTeamAction
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.lists.ProfilesImageString
import com.example.amfootball.ui.components.pages.team.ProfileTeamDialogDelte
import com.example.amfootball.ui.components.pages.team.RowButtonsProfileTeam
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.previewsMocks.ProfileTeamMocks
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.team.ProfileTeamViewModel

//TODO: Colocar mapa com a morada da Team
/**
 * Ecrã principal (Stateful) que exibe o perfil detalhado de uma equipa.
 *
 * Este Composable atua como o **Container** ou **Controller**. É responsável por:
 * 1. Coletar o estado do ViewModel (Dados da Equipa, UI State).
 * 2. Mapear as ações de navegação específicas (Editar/Apagar) e empacotá-las em [ProfileTeamAction].
 * 3. Delega a renderização para [ContentProfileTeam].
 *
 * @param navHostController Controlador de navegação para alternar entre ecrãs.
 * @param viewModel O ViewModel injetado pelo Hilt que fornece os dados da equipa e gere chamadas de API.
 */
@Composable
fun ProfileTeamScreen(
    navHostController: NavHostController,
    viewModel: ProfileTeamViewModel = hiltViewModel()
) {
    val profileTeam = viewModel.uiInfoTeam.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val role by viewModel.role.collectAsStateWithLifecycle()

    ContentProfileTeam(
        profileInfo = profileTeam.value,
        uiState = uiState,
        profileTeamAction = ProfileTeamAction(
            onEditClick = {
                viewModel.updateTeam(
                    onSucess = {
                        navHostController.navigate("${Routes.TeamRoutes.UPDATE_TEAM.route}/${profileTeam.value.id}") {
                            launchSingleTop = true
                        }
                    }
                )
            },
            onDeleteClick = {
                viewModel.deleteTeam(
                    onSucess = {
                        navHostController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                            popUpTo(0) {
                                inclusive = true
                            }
                            launchSingleTop = true
                        }
                    }
                )
            },
            retry = { viewModel.retry() }
        ),
        role = role,
        modifier = Modifier.padding(all = 16.dp)
    )
}

/**
 * Conteúdo visual (Stateless) do perfil da equipa.
 *
 * Este componente gere o estado de UI transient (como a visibilidade do [ProfileTeamDialogDelte])
 * mas não executa lógica de negócio.
 *
 * @param profileInfo O objeto de transferência de dados (DTO) com as informações da equipa.
 * @param uiState O estado atual da interface (se está a carregar ou se deu erro).
 * @param profileTeamAction Objeto que agrupa todas as callbacks de edição, eliminação e recarga.
 * @param modifier Modificadores de layout.
 */
@Composable
private fun ContentProfileTeam(
    profileInfo: ProfileTeamDto,
    uiState: UiState,
    profileTeamAction: ProfileTeamAction,
    role: UserRole,
    modifier: Modifier = Modifier
) {
    var showDeleteDialog by remember { mutableStateOf(false) }

    if (showDeleteDialog) {
        ProfileTeamDialogDelte(
            onDeleteClick = profileTeamAction.onDeleteClick,
            onDismiss = { showDeleteDialog = false }
        )
    }

    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = profileTeamAction.retry,
        content = {
            Column(
                modifier = modifier.verticalScroll(rememberScrollState()),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.Center
            ) {
                FieldProfileTeam(profileInfo = profileInfo)

                Spacer(modifier = Modifier.height(32.dp))

                if (role == UserRole.ADMIN_TEAM) {
                    RowButtonsProfileTeam(
                        onEditClick = profileTeamAction.onEditClick,
                        onDeleteClick = { showDeleteDialog = true }
                    )
                }

                Spacer(modifier = Modifier.height(24.dp))
            }
        }
    )
}

/**
 * Sub-componente responsável por renderizar os campos de texto e imagem da equipa (dados de leitura).
 *
 * @param profileInfo Os dados completos da equipa a serem apresentados nos campos.
 */
@Composable
private fun FieldProfileTeam(
    profileInfo: ProfileTeamDto
) {
    ProfilesImageString(
        image = profileInfo.logo,
        contentDescription = stringResource(id = R.string.description_logo_team, profileInfo.name),
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
    name = "1. Admin Team - PT",
    group = "Admin View",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "1. Admin Team - EN",
    group = "Admin View",
    locale = "en",
    showBackground = true
)
@Composable
fun ProfileTeamScreenAdminPreview() {
    AMFootballTheme {
        ContentProfileTeam(
            profileInfo = ProfileTeamMocks.dummyTeam,
            uiState = UiState(isLoading = false, errorMessage = null),
            profileTeamAction = ProfileTeamAction(
                onEditClick = {},
                onDeleteClick = {},
                retry = {}
            ),
            role = UserRole.ADMIN_TEAM,
            modifier = Modifier.padding(16.dp)
        )
    }
}

@Preview(
    name = "2. Member Team - PT",
    group = "Member View",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "2. Member Team - EN",
    group = "Member View",
    locale = "en",
    showBackground = true
)
@Composable
fun ProfileTeamScreenMemberPreview() {
    AMFootballTheme {
        ContentProfileTeam(
            profileInfo = ProfileTeamMocks.dummyTeam,
            uiState = UiState(isLoading = false, errorMessage = null),
            profileTeamAction = ProfileTeamAction(
                onEditClick = {},
                onDeleteClick = {},
                retry = {}
            ),
            role = UserRole.MEMBER_TEAM,
            modifier = Modifier.padding(16.dp)
        )
    }
}