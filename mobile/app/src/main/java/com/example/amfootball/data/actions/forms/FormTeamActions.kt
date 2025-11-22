package com.example.amfootball.data.actions.forms

import android.net.Uri

/**
 * Data class que agrupa as ações lambda necessárias para manipular o estado de um formulário de Equipa.
 *
 * É tipicamente usado para passar as funções de modificação de estado de todos os campos (nome, descrição, imagem, campo de jogo)
 * da ViewModel para o componente Composable.
 *
 * @property onNameChange Callback para atualizar o nome da equipa (String).
 * @property onDescriptionChange Callback para atualizar a descrição da equipa (String, pode ser null).
 * @property onImageChange Callback para atualizar a URI da imagem/logo da equipa (Uri?, pode ser null).
 * @property onNamePitchChange Callback para atualizar o nome do campo de jogo (String).
 * @property onAddressPitchChange Callback para atualizar o endereço do campo de jogo (String).
 */
data class FormTeamActions(
    val onNameChange: (String) -> Unit,
    val onDescriptionChange: (String?) -> Unit,
    val onImageChange: (Uri?) -> Unit,
    val onNamePitchChange: (String) -> Unit,
    val onAddressPitchChange: (String) -> Unit
)