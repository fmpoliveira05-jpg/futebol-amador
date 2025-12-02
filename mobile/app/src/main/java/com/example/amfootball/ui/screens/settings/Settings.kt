package com.example.amfootball.ui.screens.settings

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.amfootball.R
import com.example.amfootball.ui.components.Loading
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.viewModel.SettingsViewModel

// Adicionado LIGHT_MODE para suportar as 3 opções pedidas
enum class AppTheme {
    LIGHT_MODE, DARK_MODE, SYSTEM_DEFAULT
}

enum class AppLanguage(val code: String) {
    ENGLISH("en"),
    PORTUGUESE("pt-PT");
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    navController: NavController,
    settingsViewModel: SettingsViewModel,
    modifier: Modifier = Modifier
) {
    val showDeleteDialog =  settingsViewModel.deleteProfileState.collectAsState()
    val isLoading = settingsViewModel.isLoading.collectAsState()


    DeleteProfileDialog(showDeleteDialog, settingsViewModel)
    LoadingPage(
        isLoading = isLoading.value,
        content = {
            SettingsPageContent(modifier, settingsViewModel)
        },
        errorMsg = null,
        retry = {}
    )

}

@Composable
private fun SettingsPageContent(modifier: Modifier,settingsViewModel: SettingsViewModel){
    val currentTheme = settingsViewModel.theme.collectAsState()
    val currentLanguage = settingsViewModel.language.collectAsState()
    var notificationsEnabled by remember { mutableStateOf(true) }

    Scaffold(

    ) { innerPadding ->
        LazyColumn(
            modifier = modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(24.dp)
        ) {
            item {
                SettingsSectionTitle(title = stringResource(id = R.string.app_theme))
                ThemeSection(
                    selectedTheme = currentTheme.value,
                    settingsViewModel = settingsViewModel
                )
            }
            item {
                HorizontalDivider()
                Spacer(modifier = Modifier.height(16.dp))
                SettingsSectionTitle(title = stringResource(id = R.string.app_language))
                LanguageSection(
                    selectedLanguage = currentLanguage.value,
                    settingsViewModel = settingsViewModel

                )
            }

            // Seção de Notificações
            item {
                HorizontalDivider()
                Spacer(modifier = Modifier.height(16.dp))
                SettingsSectionTitle(title = stringResource(id = R.string.notifications))
                NotificationSection(
                    isEnabled = notificationsEnabled,
                    onToggle = { notificationsEnabled = it }
                )
            }

            // Seção de Perfil
            item {
                HorizontalDivider()
                Spacer(modifier = Modifier.height(16.dp))
                SettingsSectionTitle(title = stringResource(id = R.string.profile))
                ProfileSection(
                    onEditClick = {
                        //TODO: ir para pagina de editar perfil
                    },
                    onDeleteClick = {
                        settingsViewModel.showDeleteProfile()
                        //TODO: ir para pagina de eliminar perfil
                    }
                )
            }
        }

    }
}

@Composable
fun SettingsSectionTitle(title: String) {
    Text(
        text = title,
        style = MaterialTheme.typography.titleMedium,
        color = MaterialTheme.colorScheme.primary,
        fontWeight = FontWeight.Bold,
        modifier = Modifier.padding(bottom = 8.dp)
    )
}

@Composable
private fun ThemeSection(
    selectedTheme: String,
    settingsViewModel: SettingsViewModel
) {
    Column {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .fillMaxWidth()
                .clickable { settingsViewModel.changeTheme(AppTheme.LIGHT_MODE) }
        ) {
            RadioButton(
                selected = (selectedTheme == AppTheme.LIGHT_MODE.name),
                onClick = {
                    settingsViewModel.changeTheme(AppTheme.LIGHT_MODE)
                }
            )
            Text(
                text = stringResource(R.string.light_mode),
                modifier = Modifier.padding(start = 8.dp)
            )
        }

        // Dark Mode
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .fillMaxWidth()
                .clickable { settingsViewModel.changeTheme(AppTheme.DARK_MODE) }
        ) {
            RadioButton(
                selected = (selectedTheme == AppTheme.DARK_MODE.name),
                onClick = {settingsViewModel.changeTheme(AppTheme.DARK_MODE)}
            )
            Text(
                text = stringResource(id = R.string.darkMode),
                modifier = Modifier.padding(start = 8.dp)
            )
        }

        // Same then system
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .fillMaxWidth()
                .clickable {
                    settingsViewModel.changeTheme(AppTheme.SYSTEM_DEFAULT)
                }
        ) {
            RadioButton(
                selected = (selectedTheme == AppTheme.SYSTEM_DEFAULT.name),
                onClick = {
                    settingsViewModel.changeTheme(AppTheme.SYSTEM_DEFAULT)
                }
            )
            Text(
                text = stringResource(id = R.string.theme_same_then_system),
                modifier = Modifier.padding(start = 8.dp)
            )
        }
    }
}

@Composable
private fun LanguageSection(
    selectedLanguage: String?,
    settingsViewModel: SettingsViewModel
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceEvenly
    ) {
        val currentCode = selectedLanguage ?: AppLanguage.ENGLISH.code

        // Chip para Inglês
        FilterChip(
            selected = currentCode == AppLanguage.ENGLISH.code,
            onClick = {
                settingsViewModel.changeLanguage(AppLanguage.ENGLISH)

                      },
            label = { Text(stringResource(id = R.string.language_english)) }
        )

        // Chip para Português
        FilterChip(
            selected = currentCode == AppLanguage.PORTUGUESE.code,
            onClick = { settingsViewModel.changeLanguage(AppLanguage.PORTUGUESE) },
            label = { Text(stringResource(id = R.string.language_portuguese)) }
        )
    }
}
@Composable
private fun NotificationSection(
    isEnabled: Boolean,
    onToggle: (Boolean) -> Unit
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(text = stringResource(R.string.activate_notifications))
        Switch(
            checked = isEnabled,
            onCheckedChange = onToggle
        )
    }
}

@Composable
private fun ProfileSection(
    onEditClick: () -> Unit,
    onDeleteClick: () -> Unit
) {
    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
        Button(
            onClick = onEditClick,
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondaryContainer, contentColor = MaterialTheme.colorScheme.onSecondaryContainer)
        ) {
            Icon(Icons.Default.Edit, contentDescription = null, modifier = Modifier.size(18.dp))
            Spacer(modifier = Modifier.width(8.dp))
            Text(stringResource(id = R.string.profile_edit))
        }

        // Botão Deletar (Cor de erro para destaque)
        OutlinedButton(
            onClick = onDeleteClick,
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.error)
        ) {
            Icon(Icons.Default.Delete, contentDescription = null, modifier = Modifier.size(18.dp))
            Spacer(modifier = Modifier.width(8.dp))
            Text(stringResource(id = R.string.profile_delete))
        }
    }
}

@Composable
private fun DeleteProfileDialog(showDeleteDialog: State<Boolean>, settingsViewModel: SettingsViewModel){
    if (showDeleteDialog.value) {
        AlertDialog(
            onDismissRequest = { settingsViewModel.hideDeleteProfile() },
            icon = { Icon(Icons.Default.Warning, contentDescription = null) },
            title = {
                Text(text = stringResource(id = R.string.profile_delete))
            },
            text = {
                Text(text = stringResource(id = R.string.delete_profile_confirmation))
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        settingsViewModel.hideDeleteProfile()
                    },
                    colors = ButtonDefaults.textButtonColors(contentColor = MaterialTheme.colorScheme.error)
                ) {
                    Text(text = stringResource(id = R.string.delete_profile_action))
                }
            },
            dismissButton = {
                TextButton(
                    onClick = { settingsViewModel.hideDeleteProfile() }
                ) {
                    Text(text = stringResource(id = R.string.cancel))
                }
            }
        )
    }
}