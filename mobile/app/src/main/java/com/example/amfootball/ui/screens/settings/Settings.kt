package com.example.amfootball.ui.screens.settings

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
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
import androidx.compose.ui.window.Popup
import androidx.navigation.NavController
import com.example.amfootball.R

// Adicionado LIGHT_MODE para suportar as 3 opções pedidas
enum class AppTheme {
    LIGHT_MODE, DARK_MODE, SYSTEM_DEFAULT
}

enum class AppLanguage(string: String) {
    ENGLISH("en"), PORTUGUESE("pt-rPT")
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    navController: NavController,
    currentTheme: AppTheme,
    onThemeChanged: (AppTheme) -> Unit,
    currentLanguage: AppLanguage,
    onLanguageChanged: (AppLanguage) -> Unit,
    //onDeleteAccount: () -> Unit,
    modifier: Modifier = Modifier
) {
    var showDeleteDialog by remember { mutableStateOf(false) }
    // Estados locais (que não afetam o app global) mantêm-se aqui (ex: notificações apenas UI por enquanto)
    var notificationsEnabled by remember { mutableStateOf(true) }

    if (showDeleteDialog) {
        AlertDialog(
            onDismissRequest = { showDeleteDialog = false },
            icon = { Icon(Icons.Default.Warning, contentDescription = null) },
            title = {
                Text(text = stringResource(id = R.string.profile_delete)) // Ou "Tem a certeza?"
            },
            text = {
                Text(text = stringResource(id = R.string.delete_profile_confirmation))
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        showDeleteDialog = false
                        //onDeleteAccount() // Chama a função que vai ao endpoint
                    },
                    colors = ButtonDefaults.textButtonColors(contentColor = MaterialTheme.colorScheme.error)
                ) {
                    Text(text = stringResource(id = R.string.delete_profile_action))
                }
            },
            dismissButton = {
                TextButton(
                    onClick = { showDeleteDialog = false }
                ) {
                    Text(text = stringResource(id = R.string.cancel))
                }
            }
        )
    }
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(stringResource(R.string.item_settings)) },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = stringResource(R.string.button_back))
                    }
                }
            )
        }
    ) { innerPadding ->
        LazyColumn(
            modifier = modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(24.dp)
        ) {
            // Seção de Tema
            item {
                SettingsSectionTitle(title = stringResource(id = R.string.app_theme))
                ThemeSection(
                    selectedTheme = currentTheme,
                    onThemeSelected = onThemeChanged
                )
            }

            // Seção de Linguagem
            item {
                HorizontalDivider()
                Spacer(modifier = Modifier.height(16.dp))
                SettingsSectionTitle(title = stringResource(id = R.string.app_language))
                LanguageSection(
                    selectedLanguage = currentLanguage, // Usa o estado recebido
                    onLanguageSelected = onLanguageChanged // Chama a função recebida
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
                        showDeleteDialog = true
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
    selectedTheme: AppTheme,
    onThemeSelected: (AppTheme) -> Unit
) {
    Column {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .fillMaxWidth()
                .clickable { onThemeSelected(AppTheme.LIGHT_MODE) }
        ) {
            RadioButton(
                selected = (selectedTheme == AppTheme.LIGHT_MODE),
                onClick = { onThemeSelected(AppTheme.LIGHT_MODE) }
            )
            Text(
                text = stringResource(R.string.light_mode),
                modifier = Modifier.padding(start = 8.dp)
            )
        }

        // Opção: Dark Mode (Escuro)
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .fillMaxWidth()
                .clickable { onThemeSelected(AppTheme.DARK_MODE) }
        ) {
            RadioButton(
                selected = (selectedTheme == AppTheme.DARK_MODE),
                onClick = { onThemeSelected(AppTheme.DARK_MODE) }
            )
            Text(
                text = stringResource(id = R.string.darkMode),
                modifier = Modifier.padding(start = 8.dp)
            )
        }

        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .fillMaxWidth()
                .clickable { onThemeSelected(AppTheme.SYSTEM_DEFAULT) }
        ) {
            RadioButton(
                selected = (selectedTheme == AppTheme.SYSTEM_DEFAULT),
                onClick = { onThemeSelected(AppTheme.SYSTEM_DEFAULT) }
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
    selectedLanguage: AppLanguage,
    onLanguageSelected: (AppLanguage) -> Unit
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceEvenly
    ) {
        // Chip para Inglês
        FilterChip(
            selected = selectedLanguage == AppLanguage.ENGLISH,
            onClick = { onLanguageSelected(AppLanguage.ENGLISH) },
            label = { Text(stringResource(id = R.string.language_english)) }
        )

        // Chip para Português
        FilterChip(
            selected = selectedLanguage == AppLanguage.PORTUGUESE,
            onClick = { onLanguageSelected(AppLanguage.PORTUGUESE) },
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