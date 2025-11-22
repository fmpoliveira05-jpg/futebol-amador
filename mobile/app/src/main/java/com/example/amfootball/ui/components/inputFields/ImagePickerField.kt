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
 * Um componente Composable para selecionar uma imagem da galeria do dispositivo,
 * renderizado como um círculo.
 *
 * O componente exibe a imagem selecionada (se houver) ou um ícone de placeholder
 * (AddAPhoto). Ao ser clicado, ele lança o Photo Picker do sistema Android.
 *
 * @param modifier Modificador para estilizar o componente (tamanho, preenchimento, etc.).
 * @param imageSelectedUri A URI da imagem atualmente selecionada. Se for null, o placeholder é exibido.
 * @param onImageSelected Callback executado quando uma URI é retornada pelo Photo Picker.
 * A URI pode ser null se o usuário cancelar a seleção.
 * @param contentDescription Descrição de conteúdo para acessibilidade da imagem exibida.
 * Deve ser fornecida apenas quando [imageSelectedUri] não for null.
 * @param contentDescriptionWithoutImage Descrição de conteúdo para acessibilidade do ícone
 * [Icons.Default.AddAPhoto] (placeholder).
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