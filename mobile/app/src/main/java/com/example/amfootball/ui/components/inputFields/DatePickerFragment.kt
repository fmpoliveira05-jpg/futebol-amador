package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material3.DatePicker
import androidx.compose.material3.DatePickerState
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.SelectableDates
import androidx.compose.material3.Text
import androidx.compose.material3.rememberDatePickerState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Popup
import com.example.amfootball.R
import java.util.Calendar

/**
 * Um componente DatePicker que permite selecionar apenas datas presentes (hoje) ou futuras.
 *
 * Útil para agendamentos, prazos ou eventos futuros.
 * Internamente, calcula o timestamp do início do dia atual para validar a seleção.
 *
 * @param value A string que representa a data formatada a ser exibida no campo de texto.
 * @param onDateSelected Callback executado quando uma data válida é selecionada. Retorna o valor em milissegundos.
 * @param label O rótulo do campo de texto (ex: "Data de Vencimento").
 * @param contentDescription Descrição para acessibilidade do ícone de calendário.
 * @param isSingleLine Define se o campo de texto deve ser de linha única. Padrão é false.
 * @param isError Indica se o campo está em estado de erro (borda vermelha).
 * @param errorMessage A mensagem de erro a ser exibida abaixo do campo se [isError] for true.
 * @param modifier Modificador para estilizar o componente.
 */
@Composable
fun DatePickerDockedFutureLimitedDate(
    value: String,
    onDateSelected: (millis: Long) -> Unit,
    label: String,
    contentDescription: String,
    isSingleLine: Boolean = false,
    isError: Boolean = false,
    enabled: Boolean = true,
    errorMessage: String? = stringResource(id = R.string.mandatory_field),
    modifier: Modifier = Modifier
) {
    LimitedDatePickerBase(
        value = value,
        onDateSelected = onDateSelected,
        label = label,
        contentDescription = contentDescription,
        isSingleLine = isSingleLine,
        isError = isError,
        errorMessage = errorMessage,
        enabled = enabled,
        modifier = modifier,
        validator = { dateMillis, todayMillis -> dateMillis >= todayMillis }
    )
}

/**
 * Um componente DatePicker que permite selecionar apenas datas passadas ou o dia de hoje.
 *
 * Útil para datas de nascimento, histórico de eventos ou registos passados.
 * Internamente, calcula o timestamp do início do dia atual para validar a seleção.
 *
 * @param value A string que representa a data formatada a ser exibida no campo de texto.
 * @param onDateSelected Callback executado quando uma data válida é selecionada. Retorna o valor em milissegundos.
 * @param label O rótulo do campo de texto (ex: "Data de Nascimento").
 * @param contentDescription Descrição para acessibilidade do ícone de calendário.
 * @param isSingleLine Define se o campo de texto deve ser de linha única. Padrão é false.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage A mensagem de erro a ser exibida abaixo do campo se [isError] for true.
 * @param modifier Modificador para estilizar o componente.
 */
@Composable
fun DatePickerDockedPastLimitedDate(
    value: String,
    onDateSelected: (millis: Long) -> Unit,
    label: String,
    contentDescription: String,
    isSingleLine: Boolean = false,
    isError: Boolean = false,
    errorMessage: String? = stringResource(id = R.string.mandatory_field),
    enabled: Boolean = true,
    modifier: Modifier = Modifier
) {
    LimitedDatePickerBase(
        value = value,
        onDateSelected = onDateSelected,
        label = label,
        contentDescription = contentDescription,
        isSingleLine = isSingleLine,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier,
        enabled = enabled,
        validator = { dateMillis, todayMillis -> dateMillis <= todayMillis }
    )
}

/**
 * Um componente DatePicker padrão sem restrições de data.
 *
 * Permite selecionar qualquer data (passado, presente ou futuro).
 *
 * @param value A string que representa a data formatada a ser exibida no campo de texto.
 * @param onDateSelected Callback executado quando uma data é selecionada. Retorna o valor em milissegundos.
 * @param label O rótulo do campo de texto.
 * @param modifier Modificador para estilizar o componente.
 * @param contentDescription Descrição para acessibilidade do ícone de calendário.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage A mensagem de erro a ser exibida abaixo do campo se [isError] for true.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DatePickerDocked(value: String,
                     onDateSelected: (millis: Long) -> Unit,
                     label: String,
                     modifier: Modifier = Modifier,
                     contentDescription: String,
                     isError: Boolean = false,
                     enabled: Boolean = false,
                     errorMessage: String? = null
) {
    var showDatePicker by remember { mutableStateOf(value = false) }
    val datePickerState = rememberDatePickerState()

    DatePickerImpl(
        label = label,
        value = value,
        onIconClick = { showDatePicker = !showDatePicker },
        contentDescription = contentDescription,
        showDatePicker = showDatePicker,
        datePickerState = datePickerState,
        onDismiss = { showDatePicker = false },
        onDateSelected = onDateSelected,
        isError = isError,
        errorMessage = errorMessage,
        enabled = enabled,
        modifier = modifier
    )
}

/**
 * Base privada para DatePickers com validação de limites (Passado ou Futuro).
 *
 * Centraliza a lógica de inicialização do calendário (zerando horas/minutos para obter o "hoje")
 * e configura o [SelectableDates] com base no validador fornecido.
 *
 * @param validator Uma função lambda que recebe (dataSelecionada, dataDeHoje) e retorna true se a data for válida.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun LimitedDatePickerBase(
    value: String,
    onDateSelected: (millis: Long) -> Unit,
    label: String,
    contentDescription: String,
    isSingleLine: Boolean,
    isError: Boolean,
    errorMessage: String?,
    enabled: Boolean = false,
    modifier: Modifier,
    validator: (dateMillis: Long, todayMillis: Long) -> Boolean
) {
    var showDatePicker by remember { mutableStateOf(value = false) }

    val todayMillis = remember {
        Calendar.getInstance().apply {
            set(Calendar.HOUR_OF_DAY, 0)
            set(Calendar.MINUTE, 0)
            set(Calendar.SECOND, 0)
            set(Calendar.MILLISECOND, 0)
        }.timeInMillis
    }

    val selectableDates = remember(todayMillis, validator) {
        object : SelectableDates {
            override fun isSelectableDate(utcTimeMillis: Long): Boolean {
                return validator(utcTimeMillis, todayMillis)
            }
        }
    }

    val datePickerState = rememberDatePickerState(
        selectableDates = selectableDates
    )

    DatePickerImpl(
        label = label,
        value = value,
        onIconClick = { showDatePicker = !showDatePicker },
        contentDescription = contentDescription,
        showDatePicker = showDatePicker,
        datePickerState = datePickerState,
        onDismiss = { showDatePicker = false },
        onDateSelected = onDateSelected,
        isSingleLine = isSingleLine,
        isError = isError,
        enabled = enabled,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Implementação visual e estrutural do DatePicker.
 *
 * Combina o campo de texto ([DateOutlineOutlinedTextField]) com o popup flutuante ([PopUpDatePicker]).
 * Gerencia a lógica de exibição do popup e delega os eventos de seleção.
 *
 * @param showDatePicker Booleano que controla a visibilidade do popup.
 * @param datePickerState O estado do DatePicker do Material3.
 * @param onDismiss Ação para fechar o popup.
 * @param onIconClick Ação ao clicar no ícone do calendário (abrir/fechar popup).
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun DatePickerImpl(
    label: String,
    value: String,
    onIconClick: () -> Unit,
    contentDescription: String,
    showDatePicker: Boolean,
    datePickerState: DatePickerState,
    onDismiss: () -> Unit,
    onDateSelected: (millis: Long) -> Unit,
    isSingleLine: Boolean = false,
    isError: Boolean = false,
    errorMessage: String? = null,
    enabled: Boolean,
    modifier: Modifier = Modifier
){
    LaunchedEffectDatePicker(
        datePickerState = datePickerState,
        onDateSelected = onDateSelected,
        onDismiss = onDismiss
    )

    Box(
        modifier = modifier
    ) {
        DateOutlineOutlinedTextField(
            value = value,
            label = label,
            contentDescription = contentDescription,
            onIconClick = onIconClick,
            isSingleLine = isSingleLine,
            isError = isError,
            enabled = enabled,
            errorMessage = errorMessage
        )

        if (showDatePicker) {
            PopUpDatePicker(
                datePickerState = datePickerState,
                onDismiss = onDismiss
            )
        }
    }
}

/**
 * Helper que observa alterações no estado do DatePicker.
 *
 * Quando o usuário seleciona uma data no calendário, este efeito é disparado para:
 * 1. Notificar o pai via [onDateSelected].
 * 2. Fechar o popup via [onDismiss].
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun LaunchedEffectDatePicker(
    datePickerState: DatePickerState,
    onDateSelected: (millis: Long) -> Unit,
    onDismiss: () -> Unit
) {
    LaunchedEffect(datePickerState.selectedDateMillis) {
        datePickerState.selectedDateMillis?.let { millis ->
            onDateSelected(millis)
            onDismiss()
        }
    }
}

/**
 * O campo de texto "fake" que exibe a data selecionada.
 *
 * É configurado como `readOnly = true` para impedir digitação manual, forçando o uso do calendário.
 * Inclui um ícone de calendário no final (trailingIcon) que serve de gatilho para o popup.
 */
@Composable
private fun DateOutlineOutlinedTextField(
    value: String,
    label: String,
    contentDescription: String,
    onIconClick: () -> Unit,
    isSingleLine: Boolean = false,
    isError: Boolean = false,
    enabled: Boolean,
    errorMessage: String? = null,
) {
    OutlinedTextField(
        value = value,
        onValueChange = { },
        label = { Text(text = label) },
        readOnly = true,
        trailingIcon = {
            IconButton(
                onClick = onIconClick
            ) {
                Icon(
                    imageVector = Icons.Default.DateRange,
                    contentDescription = contentDescription
                )
            }
        },
        supportingText = {
            if (isError && errorMessage != null) {
                Text(
                    text = errorMessage,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodySmall
                )
            }
        },
        enabled = true,
        singleLine = isSingleLine,
        modifier = Modifier
            .fillMaxWidth()
            .heightIn(min = 64.dp)
    )
}

/**
 * O container flutuante (Popup) que abriga o calendário.
 *
 * Utiliza um [Popup] do Compose para sobrepor o conteúdo na tela, alinhado abaixo do campo de texto.
 * Possui sombra e fundo de superfície para aderir ao Material Design.
 */
@Composable
private fun PopUpDatePicker(
    onDismiss: () -> Unit,
    datePickerState: DatePickerState
) {
    Popup(
        onDismissRequest = onDismiss,
        alignment = Alignment.TopStart
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .offset(y = 64.dp)
                .shadow(elevation = 4.dp)
                .background(color = MaterialTheme.colorScheme.surface)
                .padding(all = 16.dp)
        ) {
            DatePicker(
                state = datePickerState,
                showModeToggle = true,
            )
        }
    }
}