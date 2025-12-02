package com.example.amfootball.ui.screens.user

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
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
import com.example.amfootball.data.dtos.player.FireBaseLoginResponseDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.lists.ProfilesImageString
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.user.ProfilePlayerViewModel
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PlayerConst
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.utils.UserConst

/**
 * Ecrã principal de Perfil do Jogador (Stateful).
 *
 * Este Composable é o ponto de entrada da UI para visualizar os detalhes de um jogador.
 * Ele é responsável por:
 * 1. Obter a instância do [ProfilePlayerViewModel] através de injeção de dependência (Hilt).
 * 2. Observar o estado da UI ([uiState]) e os dados do perfil ([profile]).
 * 3. Gerir os estados de carregamento e erro utilizando o componente [LoadingPage].
 * 4. Delegar o desenho da UI para o [ProfileScreenContent].
 *
 * @param viewModel O ViewModel que fornece os dados e gere a lógica de negócio. Injetado automaticamente pelo Hilt.
 */
@Composable
fun ProfileScreen(
    viewModel: ProfilePlayerViewModel = hiltViewModel()
) {
    val profile by viewModel.uiProfilePlayer.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    ProfileScreenContent(
        profileData = profile,
        uiState = uiState,
        retry = { viewModel.retry() },
        modifier = Modifier.padding(16.dp)
    )
}

/**
 * Conteúdo interno do ecrã de perfil (Stateless).
 *
 * Organiza os elementos visuais numa [Column] com scroll vertical ativado.
 * Esta separação permite testar a UI isoladamente sem precisar do ViewModel.
 *
 * @param profileData O objeto de dados [PlayerProfileDto] com as informações do jogador a exibir (pode ser nulo durante o loading).
 * @param uiState O estado atual da UI (Loading, Erro, Sucesso).
 * @param retry Função para tentar recarregar os dados em caso de erro.
 * @param modifier Modificador para layout e estilização.
 */
@Composable
private fun ProfileScreenContent(
    profileData: PlayerProfileDto?,
    uiState: UiState,
    retry: () -> Unit,
    modifier: Modifier = Modifier
) {
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg= uiState.errorMessage,
        retry= { retry() },
        content = {
            if (profileData != null) {
                Column(
                    modifier = modifier
                        .fillMaxWidth()
                        .verticalScroll(rememberScrollState()), // Habilita o scroll numa Column normal
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.Center
                ) {
                    TextFieldProfile(profileData = profileData)
                }
            }
        }
    )
}

/**
 * Componente que lista os campos de detalhe do perfil.
 *
 * Apresenta a imagem do jogador e uma série de campos de texto [TextFieldOutline]
 * em modo de leitura (read-only) com as informações pessoais e desportivas.
 *
 * @param profileData Os dados do jogador a preencher nos campos.
 */
@Composable
private fun TextFieldProfile(profileData: PlayerProfileDto) {
    ProfilesImageString(
        image = profileData.icon,
        modifier = Modifier.fillMaxWidth()
    )

    TextFieldOutline(
        label = stringResource(id = R.string.player_name),
        value = profileData.name,
        minLenght = UserConst.MIN_NAME_LENGTH,
        maxLenght = UserConst.MAX_NAME_LENGTH,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.date_of_birthday),
        value = profileData.dateOfBirth,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.email_field),
        value = profileData.email,
        minLenght = UserConst.MIN_EMAIL_LENGTH,
        maxLenght = UserConst.MAX_EMAIL_LENGTH,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.phone_number),
        value = if(profileData.phoneNumber != null) {
            profileData.phoneNumber
        } else {
            profileData.loginResponseDto?.phoneNumber.toString()
        },
        minLenght = UserConst.SIZE_PHONE_NUMBER,
        maxLenght = UserConst.SIZE_PHONE_NUMBER,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.address),
        value = profileData.address,
        minLenght = GeneralConst.MIN_ADDRESS_LENGTH,
        maxLenght = GeneralConst.MAX_ADDRESS_LENGTH,
        isSingleLine = false,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.player_position),
        value = profileData.position,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.player_size),
        value = profileData.height.toString() + " cm",
        minLenght = PlayerConst.MIN_HEIGHT,
        maxLenght = PlayerConst.MAX_HEIGHT,
        isReadOnly = true
    )

    //TODO: Não aparece pois não se guarda no login, fazer pedido hhá API só para isto
    TextFieldOutline(
        label = stringResource(id = R.string.player_team),
        value = profileData.team?.name,
        minLenght = TeamConst.MIN_NAME_LENGTH,
        maxLenght = TeamConst.MAX_NAME_LENGTH,
        isReadOnly = true
    )
}

@Preview(
    name = "Preview Profile Screen PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "Preview Profile Screen Eng",
    locale = "en",
    showBackground = true
)
@Composable
fun ProfileScreenPreview() {
    val dummyLoginResponse = FireBaseLoginResponseDto(
        idToken = "token_mock",
        refreshToken = "refresh_mock",
        expiresIn = "3600",
        localId = "1",
        phoneNumber = 934567890,
        email = "lm10@email.com"
    )

    val dummyProfile = PlayerProfileDto(
        loginResponseDto = dummyLoginResponse,
        name = "Lionel Messi",
        dateOfBirth = "24/06/1987",
        icon = null,
        address = "Rosario, Argentina",
        position = "Avançado",
        height = 170,
        team = TeamDto(
            id = "2",
            name = "Inter Miami CF",
        ),
        isAdmin = false,
        email = dummyLoginResponse.email,
        phoneNumber = dummyLoginResponse.phoneNumber.toString()
    )

    AMFootballTheme {
        ProfileScreenContent(
            profileData = dummyProfile,
            uiState = UiState(isLoading = false, errorMessage = null),
            retry = {},
            modifier = Modifier.padding(16.dp)
        )
    }
}