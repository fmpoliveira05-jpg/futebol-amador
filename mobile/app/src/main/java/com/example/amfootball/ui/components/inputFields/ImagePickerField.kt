package com.example.amfootball.ui.components.inputFields

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.PickVisualMediaRequest
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AddAPhoto
import androidx.compose.material3.Icon
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage

/**
 * Componente de Interface (UI) para seleção de imagens da galeria utilizando o Photo Picker moderno.
 *
 * Este componente apresenta uma área circular interativa que serve dois propósitos:
 * 1. **Visualização:** Exibe a imagem selecionada (via Coil) ou um ícone de placeholder ("Adicionar Foto").
 * 2. **Interação:** Ao ser clicado, lança o seletor de média nativo do Android (`PickVisualMedia`),
 * garantindo privacidade e segurança (não requer permissão `READ_EXTERNAL_STORAGE` em versões recentes).
 *
 * **Características Visuais:**
 * - Formato circular fixo ([CircleShape]).
 * - Tamanho predefinido de 150.dp (ajustável via modifier).
 * - Imagens são cortadas ([ContentScale.Crop]) para preencher o círculo uniformemente.
 *
 * @param modifier Modificador para estilizar o contentor externo (margens, tamanho, etc.).
 * @param imageSelectedUri O [Uri] da imagem a ser exibida. Se for `null`, o componente mostra o ícone de placeholder.
 * @param onImageSelected Callback executado quando o seletor retorna. Recebe o [Uri] da imagem escolhida ou `null` se o utilizador cancelar.
 * @param contentDescription Texto de acessibilidade (TalkBack) para descrever a **imagem carregada** (ex: "Foto de perfil de João").
 * @param contentDescriptionWithoutImage Texto de acessibilidade para o **estado vazio** (ex: "Toque para adicionar uma foto de perfil").
 */
@Composable
fun ImagePicker(
    modifier: Modifier = Modifier,
    imageSelectedUri: Uri?,
    onImageSelected: (Uri?) -> Unit,
    contentDescription: String? = null,
    contentDescriptionWithoutImage: String? = null,
) {
    //Cria o launcher para o Photo Picker
    val photoPickerLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.PickVisualMedia(),
        onResult = { uri ->
            onImageSelected(uri)
        }
    )

    Box(
        modifier = modifier
            .size(150.dp)
            .clip(CircleShape)
            .background(Color.LightGray.copy(alpha = 0.6f))
            .border(1.dp, Color.Gray, CircleShape)
            .clickable {
                photoPickerLauncher.launch(
                    PickVisualMediaRequest(
                        ActivityResultContracts.PickVisualMedia.ImageOnly
                    )
                )
            },
        contentAlignment = Alignment.Center
    ) {
        if (imageSelectedUri != null) {
            AsyncImage(
                model = imageSelectedUri,
                contentDescription = contentDescription,
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Crop
            )
        } else {
            Icon(
                imageVector = Icons.Default.AddAPhoto,
                contentDescription = contentDescriptionWithoutImage,
                tint = Color.Gray,
                modifier = Modifier.size(48.dp)
            )
        }
    }
}