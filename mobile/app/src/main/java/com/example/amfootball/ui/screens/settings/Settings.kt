package com.example.amfootball.ui.screens.settings

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FilterChip
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.RadioButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.State
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.amfootball.R
import com.example.amfootball.data.enums.settings.AppLanguage
import com.example.amfootball.data.enums.settings.AppTheme
import com.example.amfootball.ui.viewModel.SettingsViewModel

/**
 * Ecrã de Definições e Preferências da Aplicação.
 *
 * Este ecrã permite ao utilizador configurar aspetos globais da aplicação e gerir a sua conta.
 * Está organizado em quatro secções principais numa lista rolável ([LazyColumn]):
 * 1. **Tema:** Escolha de tema visual.
 * 2. **Idioma:** Seleção de idioma.
 * 3. **Notificações:** Ativar/desativar alertas.
 * 4. **Perfil:** Ações de edição e eliminação de conta.
 *
 * @param navController Controlador de navegação (para futuros redirecionamentos, ex: Editar Perfil).
 * @param settingsViewModel ViewModel que gere o estado das preferências e persistência de dados.
 * @param modifier Modificador de layout.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    navController: NavController,
    settingsViewModel: SettingsViewModel,
    modifier: Modifier = Modifier
) {
    val showDeleteDialog = settingsViewModel.deleteProfileState.collectAsState()
    val currentTheme = settingsViewModel.theme.collectAsState()
    val currentLanguage = settingsViewModel.language.collectAsState()
    var notificationsEnabled by remember { mutableStateOf(true) }

    DeleteProfileDialog(showDeleteDialog, settingsViewModel)

    Scaffold{ innerPadding ->
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

/**
 * Título padronizado para as secções do ecrã de definições.
 *
 * @param title O texto do título a ser exibido.
 */
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

/**
 * Secção de seleção de Tema visual.
 *
 * Apresenta 3 opções mutuamente exclusivas via [RadioButton].
 *
 * @param selectedTheme O nome do tema atualmente selecionado (String correspondente ao Enum.name).
 * @param settingsViewModel ViewModel para invocar a mudança de tema.
 */
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
                .clickable { AppTheme.LIGHT_MODE }
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
                .clickable { AppTheme.DARK_MODE }
        ) {
            RadioButton(
                selected = (selectedTheme == AppTheme.DARK_MODE.name),
                onClick = { settingsViewModel.changeTheme(AppTheme.DARK_MODE) }
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
                    AppTheme.SYSTEM_DEFAULT
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

/**
 * Secção de seleção de Idioma.
 *
 * Utiliza [FilterChip] para apresentar as opções de língua lado a lado.
 *
 * @param selectedLanguage O código do idioma atualmente selecionado.
 * @param settingsViewModel ViewModel para invocar a mudança de idioma.
 */
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
            onClick = { settingsViewModel.changeLanguage(AppLanguage.ENGLISH) },
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

/**
 * Secção de configuração de Notificações.
 *
 * Exibe um [Switch] para ativar ou desativar as notificações da app.
 *
 * @param isEnabled Estado atual do switch.
 * @param onToggle Callback executado ao alterar o estado.
 */
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

/**
 * Secção de gestão de Perfil.
 *
 * Fornece botões para ações críticas da conta: Editar e Deletar.
 *
 * @param onEditClick Ação para navegar para a edição do perfil.
 * @param onDeleteClick Ação para iniciar o fluxo de eliminação (abrir diálogo).
 */
@Composable
private fun ProfileSection(
    onEditClick: () -> Unit,
    onDeleteClick: () -> Unit
) {
    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
        Button(
            onClick = onEditClick,
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.buttonColors(
                containerColor = MaterialTheme.colorScheme.secondaryContainer,
                contentColor = MaterialTheme.colorScheme.onSecondaryContainer
            )
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

/**
 * Diálogo de alerta [AlertDialog] para confirmação de eliminação de perfil.
 *
 * Exige confirmação explícita para evitar a eliminação acidental da conta.
 *
 * @param showDeleteDialog Estado observável que controla a visibilidade do diálogo.
 * @param settingsViewModel ViewModel para gerir o fecho do diálogo ou a ação de eliminação.
 */
@Composable
private fun DeleteProfileDialog(
    showDeleteDialog: State<Boolean>,
    settingsViewModel: SettingsViewModel
) {
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