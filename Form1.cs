using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MMF
{
    public partial class Form1 : Form
    {
        //Variables globales
        //Globall variables
        int numDocument = 0;
        List<Datos> ListDocuments = new List<Datos>();
        public Form1()
        {
            InitializeComponent();
        }

        //Metodo para el control de los archivos Subidos a memoria
        // Method for controlling files uploaded to memory
        public class Datos : IEquatable<Datos>
        {
            public int PartId { get; set; }
            public int PartState { get; set; }
            public string PartName { get; set; }
            public long PartSize { get; set; }

            public override string ToString()
            {
                return "ID: " + PartId + "   State: " + PartState + "   Name: " + PartName + "   Size: " + PartSize;
            }
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                Datos objAsPart = obj as Datos;
                if (objAsPart == null) return false;
                else return Equals(objAsPart);
            }
            public override int GetHashCode()
            {
                return PartId;
            }
            public bool Equals(Datos other)
            {
                if (other == null) return false;
                return (this.PartId.Equals(other.PartId));
            }
        }

        //Metodo para ver la lista de archivos en la memoria
        //Method to view the list of files in memory
        public void verLista()
        {
            string lista = "";
            foreach (Datos aPart in ListDocuments)
            {
                lista += aPart + "\n";
            }
            MessageBox.Show(lista);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //Agregamos un try para controlar los errores
            // Add a try to control errors
            try
            {
                //Conteo de documentos 
                //Document counting
                numDocument++;

                //Entrada de datos en tipo  byte
                // Data input in byte type
                byte[] fileByte = File.ReadAllBytes(@"C:\Users\Dev01\Documents\Indices.xlsx");

                //Instanciacion de metodo MemoryMappedFile para crear un espacio en memoria usando los siguientes parametros -Nombre del espcio en memoria, - tamaño del archivo, -permisos que tendra el espacio en memoria
                // Instance of the MemoryMappedFile method to create a space in memory using the following parameters - Name of the memory space, - File size, - Permissions that the memory space will have
                MemoryMappedFile mmf = MemoryMappedFile.CreateNew("Document" + numDocument, fileByte.LongLength, 0);

                //Despues creamos una instancia para poder escribir dentro del espacio en memoria con MemoryMappedFileViewStream
                // Then we create an instance to be able to write into memory space with MemoryMappedFileViewStream
                MemoryMappedViewStream stream = mmf.CreateViewStream();

                //Ensegida agragamos una instancia a la funcion BinaryWriter para que escriba lo queramos en el espacio de memoria
                //Next we add an instance to the BinaryWriter function to write what we want in the memory space
                BinaryWriter writer = new BinaryWriter(stream);

                //Aqui pasaos el areglo de bytes para que lo escriba en memoria, puede ser otro tipo de dato que ustedes necesiten el metodo .Write puede escribir varios tipos de datos
                // Here we pass the byte array to write it in memory, it can be another type of data that you need, the Write method can write several types of data
                writer.Write(fileByte);

                //Agregamos a la lista que hicimos para llevar el control de los  documentos, este es un ejemplo dependera de sus necesidades.
                // We add to the list we made to keep track of the documents, this is an example depending on your needs.
                ListDocuments.Add(new Datos() { PartId = numDocument, PartState = 0, PartName = "Document" + numDocument, PartSize = fileByte.LongLength });

                //Este metodo muestra una lista de los documentos subidos.
                // This method shows a list of uploaded documents.
                verLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: \n" + ex.Message + "\n" + ex.TargetSite);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //Agregamos un try para controlar los errores
            // Add a try to control errors
            try
            {
                //Aqui buscamos el archivo en la lista que se creo anteriormente 
                // Here we look for the file in the list that was previously created
                string path = @"C:\Temporal\Indices.xlsx";
                var DocExistente = ListDocuments.Find(s => s.PartState == (0));

                //Usamos "Using" para que cuando termine de leer, libere el espacio en memoria utilizados,
                //usamos el metodo OpenExisting para leer el espacio en memoria creado al cual le pasamos los siguientes 
                //parametros -nombre del espacio en memoria, -el tipo de control que tendremos sobre el.

                //we Use "Using" so that when you finish reading, free up the memory space used,
                //we use the OpenExisting method to read the space in created memory to which we pass the following
                //parameters -name of the space in memory, -the type of control that we will have on him.
                using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(DocExistente.PartName, MemoryMappedFileRights.FullControl))
                {
                    //Creamos un arreglo de bytes para almacenar lo que traiga el espacio en memoria, puede ser otro tipo de dato-.
                    // We create an array of bytes to store what the space brings in memory, it can be another type of data.
                    byte[] fileByte;

                    //creamos una instancia para el metodo createViewStream
                    // create an instance for the createViewStream method
                    MemoryMappedViewStream stream = mmf.CreateViewStream();

                    //Utilizando el BinaryReader obtendremos lo que esta escrito en la memoria
                    // Using the BinaryReader we will get what is written in memory
                    BinaryReader reader = new BinaryReader(stream);

                    //Despues lo pasamos a nuestro arreglo de bytes
                    // Then we pass it to our byte array
                    fileByte = reader.ReadBytes(Convert.ToInt32(DocExistente.PartSize));

                    //Como es un archivo lo que esta en memoria creamos uno para escribirlo en el disco.    
                    // As a file what is in memory we create one to write it to disk
                    using (File.Create(path))
                    {
                    }
                    File.WriteAllBytes(path, fileByte);

                    //Para el control lo quitamos de la lista y lo agregamos con  un PrtState=1 de leido o modificado etc.
                    // For the control we remove it from the list and add it with a PrtState = 1 of read or modified etc.
                    ListDocuments.Remove(new Datos() { PartId = DocExistente.PartId });
                    ListDocuments.Add(new Datos() { PartId = DocExistente.PartId, PartState = 1, PartName = DocExistente.PartName, PartSize = DocExistente.PartSize });
                }
                //Mostramos resultados
                //We Show result
                verLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No hay archivos disponibles");
            }
        }
    }
}
