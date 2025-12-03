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
import androidx.compose.ui.platform.testTag
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
    modifier: Modifier = Modifier,
    textFieldModifier: Modifier = Modifier
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
        validator = { dateMillis, todayMillis -> dateMillis >= todayMillis },
        textFieldModifier = textFieldModifier
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
fun DatePickerDocked(
    value: String,
    onDateSelected: (millis: Long) -> Unit,
    label: String,
    modifier: Modifier = Modifier,
    contentDescription: String,
    isError: Boolean = false,
    enabled: Boolean = false,
    errorMessage: String? = null,
    textFieldModifier: Modifier = Modifier
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
        modifier = modifier,
        textFieldModifier = textFieldModifier
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
    textFieldModifier: Modifier = Modifier,
    validator: (dateMillis: Long, todayMillis: Long) -> Boolean,
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
        onIconClick = {
            if (enabled) {
                showDatePicker = !showDatePicker
            }
        },
        contentDescription = contentDescription,
        showDatePicker = showDatePicker,
        datePickerState = datePickerState,
        onDismiss = { showDatePicker = false },
        onDateSelected = onDateSelected,
        isSingleLine = isSingleLine,
        isError = isError,
        enabled = enabled,
        errorMessage = errorMessage,
        modifier = modifier,
        textFieldModifier = textFieldModifier
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
    modifier: Modifier = Modifier,
    textFieldModifier: Modifier = Modifier
) {
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
            errorMessage = errorMessage,
            textFieldModifier = textFieldModifier
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
 * Efeito colateral (Side-effect) que monitoriza a seleção de datas no estado do calendário.
 *
 * Este componente não renderiza UI, mas atua como um observador. Assim que o [datePickerState]
 * regista uma nova data selecionada (`selectedDateMillis` deixa de ser null ou muda),
 * este efeito é disparado para propagar o valor e fechar o popup.
 *
 * @param datePickerState O estado do Material3 DatePicker que contém a data selecionada.
 * @param onDateSelected Callback executado com o valor da data em milissegundos (Long).
 * @param onDismiss Callback para fechar o popup/diálogo após a seleção.
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
 * Componente visual de campo de texto configurado especificamente para seleção de datas.
 *
 * Funciona como um "Trigger" para o calendário. Embora pareça um campo de texto normal,
 * está configurado como `readOnly` (quando habilitado) para impedir a inserção manual de texto,
 * garantindo que o formato da data é sempre consistente através da seleção no calendário.
 *
 * Inclui suporte para estados de erro e mensagens de validação.
 *
 * @param value O texto da data formatada a ser exibido.
 * @param label O rótulo (hint) do campo.
 * @param contentDescription Descrição para acessibilidade do ícone de calendário.
 * @param onIconClick Ação a executar ao clicar no ícone (geralmente abrir o DatePicker).
 * @param isSingleLine Se o texto deve ser mantido numa única linha (Padrão: false).
 * @param isError Indica se o campo deve ser pintado com a cor de erro.
 * @param enabled Controla se o campo está ativo. Se `true`, define `readOnly = true`.
 * @param errorMessage Mensagem de texto a exibir abaixo do campo em caso de erro.
 * @param textFieldModifier Modificadores de layout adicionais.
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
    textFieldModifier: Modifier = Modifier
) {
    OutlinedTextField(
        value = value,
        onValueChange = { },
        label = { Text(text = label) },
        readOnly = enabled,
        trailingIcon = {
            IconButton(
                onClick = onIconClick,
                modifier = Modifier.then(textFieldModifier)
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
        enabled = enabled,
        singleLine = isSingleLine,
        modifier = Modifier
            .fillMaxWidth()
            .heightIn(min = 64.dp)
            .then(textFieldModifier)
    )
}

/**
 * Contentor flutuante (Popup) que encapsula o componente de calendário.
 *
 * Utiliza a primitiva [Popup] do Jetpack Compose para renderizar o calendário numa camada superior (Z-index),
 * posicionando-o estrategicamente abaixo do campo de texto.
 *
 * O design inclui um fundo de superfície (`Surface`) e sombra (`Shadow`) para destacar o calendário
 * do resto do conteúdo do formulário.
 *
 * @param onDismiss Callback invocado quando o utilizador clica fora da área do popup (dismiss request).
 * @param datePickerState O estado que controla a data selecionada e a visualização do calendário interno.
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
                .testTag("date_picker_popup")
        ) {
            DatePicker(
                state = datePickerState,
                showModeToggle = true,
            )
        }
    }
}