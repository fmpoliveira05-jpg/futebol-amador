package com.example.amfootball.ui.screens.user

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
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
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.lists.ProfilesImageString
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.user.ProfilePlayerViewModel
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PlayerConst
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.utils.UserConst

@Composable
fun ProfileScreen(
    viewModel: ProfilePlayerViewModel = hiltViewModel()
) {
    val profile by viewModel.uiProfilePlayer.collectAsStateWithLifecycle()
    val isLoading by viewModel.isLoading.collectAsStateWithLifecycle()
    val errorMsg by viewModel.errorMessage.collectAsStateWithLifecycle()

    LoadingPage(
        isLoading = isLoading,
        errorMsg= errorMsg,
        retry= { viewModel.retry() },
        content = {
            if(profile != null) {
                ProfileScreenContent(
                    profileData = profile!!,
                    modifier = Modifier
                        .padding(16.dp)
                )
            }
        }
    )
}

@Composable
private fun ProfileScreenContent(
    profileData: PlayerProfileDto,
    modifier: Modifier = Modifier
) {
    LazyColumn(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        item {
            TextFieldProfile(profileData = profileData)
        }
    }
}

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
        value = profileData.phoneNumber,
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

    TextFieldOutline(
        label = stringResource(id = R.string.player_team),
        value = profileData.team,
        minLenght = TeamConst.MIN_NAME_LENGTH,
        maxLenght = TeamConst.MAX_NAME_LENGTH,
        isReadOnly = true
    )
}

@Preview(name = "Preview Profile Screen PT",
    locale = "pt",
    showBackground = true)
@Preview(name = "Preview Profile Screen Eng",
        locale = "en",
        showBackground = true)
@Composable
fun ProfileScreenPreview() {
    AMFootballTheme {
        ProfileScreen()
    }
}