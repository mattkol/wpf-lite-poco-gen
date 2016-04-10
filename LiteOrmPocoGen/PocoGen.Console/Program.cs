// -----------------------------------------------------------------------
// <copyright file="Column.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Console
{
    using System;

    /// <summary>
    /// This class represents Program class.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            var readerParameters = new ReaderParameters();
            var result = readerParameters.ParseArguments(args);

            switch (result)
            {
                case ParserResult.Invalid:
                    Console.Write(readerParameters.Usage);
                    break;
                case ParserResult.Failure:
                    Console.Write(readerParameters.ErrorMessage);
                    break;
                case ParserResult.Success:
                    try
                    {
                        ModelsGenerator.Generate(readerParameters);
                        Console.WriteLine("Completed successfully.");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                        Console.WriteLine(exception.StackTrace);
                    }
  
                    break;
            }

            Console.ReadKey();
        }
    }
}
