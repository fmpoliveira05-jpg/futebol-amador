package com.example.amfootball.ui.screens.user

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
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
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.lists.ProfilesImage
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.user.ProfilePlayerViewModel

@Composable
fun ProfileScreen(
    navController: NavHostController,
    viewModel: ProfilePlayerViewModel = viewModel()
) {
    val profileData by viewModel.uiProfilePlayer.observeAsState(initial = PlayerProfileDto.createExample())

    ProfileScreenContent(
        profileData = profileData,
        modifier = Modifier
            .padding(16.dp)
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
    ProfilesImage(
        image = profileData.icon,
        modifier = Modifier.fillMaxWidth()
    )

    TextFieldOutline(
        label = stringResource(id = R.string.player_name),
        value = profileData.name,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.date_of_birthday),
        value = profileData.dateOfBirthday.toString(),
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.email_field),
        value = profileData.email,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.phone_number),
        value = profileData.phoneNumber,
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.address),
        value = profileData.address,
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
        value = profileData.size.toString() + " cm",
        isReadOnly = true
    )

    TextFieldOutline(
        label = stringResource(id = R.string.player_team),
        value = profileData.team,
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
        ProfileScreen(navController = rememberNavController())
    }
}