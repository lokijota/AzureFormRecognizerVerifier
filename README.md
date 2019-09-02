# Azure Form Recognizer Verifier

Small command-line tool that draws over a source image the output of Azure's Form Recognizer output, which must have been obtained from a previous call. This will allow you to easity verify what the service is recognizing in a processed form.

The form recognizer's documentation home is https://azure.microsoft.com/en-in/services/cognitive-services/form-recognizer/ .

The tool uses ImageMagick to manipulate the images and Newtonsoft.Json to parse the Json file.

TO-DO: add support for tables.
