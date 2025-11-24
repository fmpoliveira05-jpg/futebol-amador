package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowDropDown
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import com.example.amfootball.utils.CountryCodeHelper
import com.example.amfootball.utils.CountryData
import com.example.amfootball.R

/**
 * Componente de entrada de número de telefone com seletor dinâmico de código de país.
 *
 * Combina um [OutlinedTextField] para o número com um botão de prefixo que abre um [Dialog]
 * com a lista de países gerada dinamicamente (bandeira + código).
 *
 * @param phoneNumber O número de telefone atual (sem o código do país).
 * @param onPhoneNumberChange Callback executado quando o utilizador altera o número.
 * @param initialCountryCode O código de país inicial a ser selecionado (ex: "+351"). Padrão: "+351".
 * @param onCountryCodeChange Callback executado quando o utilizador seleciona um novo país da lista. Retorna o objeto [CountryData].
 * @param label O rótulo do campo. Padrão: "Telemóvel".
 * @param isRequired Se true, adiciona um asterisco vermelho ao rótulo para indicar obrigatoriedade.
 * @param isError Se true, o campo é desenhado com a cor de erro (vermelho) e exibe a mensagem de erro.
 * @param errorMessage A mensagem de erro a ser exibida abaixo do campo se [isError] for true.
 * @param modifier Modificador para estilizar o layout do componente.
 */
@Composable
fun PhoneInputWithDynamicCountries(
    phoneNumber: String,
    onPhoneNumberChange: (String) -> Unit,
    initialCountryCode: String = "+351",
    onCountryCodeChange: (CountryData) -> Unit = {},
    label: String = "Telemóvel",
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String? = stringResource(id = R.string.mandatory_field),
    modifier: Modifier = Modifier
) {
    val countries = remember { CountryCodeHelper.getCountries() }

    var selectedCountry by remember {
        mutableStateOf(countries.find { it.dialCode == initialCountryCode } ?: countries.first())
    }

    var showDialog by remember { mutableStateOf(false) }

    val labelContent: @Composable () -> Unit = {
        if (isRequired) {
            Row {
                Text(text = label)
                Text(text = " *", color = MaterialTheme.colorScheme.error)
            }
        } else {
            Text(text = label)
        }
    }

    OutlinedTextField(
        value = phoneNumber,
        onValueChange = { if (it.all { char -> char.isDigit() }) onPhoneNumberChange(it) },
        label = { Text("Telemóvel") },
        leadingIcon = {
            TextButton(
                onClick = { showDialog = true },
                contentPadding = PaddingValues(horizontal = 0.dp)
            ) {
                Text(
                    text = "${selectedCountry.flagEmoji} ${selectedCountry.dialCode}",
                    style = MaterialTheme.typography.bodyLarge,
                    color = MaterialTheme.colorScheme.onSurface
                )
                Icon(
                    imageVector = Icons.Default.ArrowDropDown,
                    contentDescription = "Select Country",
                    tint = MaterialTheme.colorScheme.onSurface
                )
            }
        },
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
        singleLine = true,
        isError = isError,
        supportingText = {
            if (isError && errorMessage != null) {
                Text(
                    text = errorMessage,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodySmall
                )
            }
        },
        modifier = modifier
    )

    if (showDialog) {
        CountrySelectionDialog(
            countries = countries,
            onDismiss = { showDialog = false },
            onCountrySelected = {
                selectedCountry = it
                onCountryCodeChange(it)
                showDialog = false
            }
        )
    }
}

/**
 * Diálogo modal que apresenta uma lista rolável de países para seleção.
 *
 * @param countries A lista de dados dos países ([CountryData]) a exibir.
 * @param onDismiss Callback para fechar o diálogo sem selecionar.
 * @param onCountrySelected Callback executado quando um país é clicado.
 */
@Composable
fun CountrySelectionDialog(
    countries: List<CountryData>,
    onDismiss: () -> Unit,
    onCountrySelected: (CountryData) -> Unit
) {
    Dialog(onDismissRequest = onDismiss) {
        Surface(
            shape = MaterialTheme.shapes.medium,
            modifier = Modifier
                .fillMaxWidth()
                .heightIn(max = 400.dp)
        ) {
            Column {
                Text(
                    text = "Selecionar País",
                    style = MaterialTheme.typography.titleMedium,
                    modifier = Modifier.padding(16.dp)
                )
                Divider()

                LazyColumn {
                    items(countries) { country ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable { onCountrySelected(country) }
                                .padding(16.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text(
                                text = country.flagEmoji,
                                style = MaterialTheme.typography.headlineSmall
                            )
                            Spacer(modifier = Modifier.width(16.dp))
                            Text(
                                text = "${country.name} (${country.dialCode})",
                                style = MaterialTheme.typography.bodyMedium
                            )
                        }
                        Divider(color = MaterialTheme.colorScheme.surfaceVariant, thickness = 0.5.dp)
                    }
                }
            }
        }
    }
}