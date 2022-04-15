# Tutorial Guideline

To begin, we'll download a tool provided by stratis that will enable us to run a full node locally (a contatiner which contains all of the blockchain's data) with a GUI for deploying smart contracts.

## Installation

Navigate to 

```bash
https://github.com/stratisproject/CirrusCore/releases/tag/1.6.1.0-privatenet
```
and scroll down to the assets section, and download 'Cirrus.Core.Private.Net-v1.6.1-setup-win-x64.exe' if you're a Windows user, or 'Cirrus.Core.Private.Net-v1.6.1-mac.dmg' if you are a mac user

## Usage

Open the 'Cirrus Core Private Net' application and wait for the node to be started by the program.

Log in with the account name 'cirrusdev' and the password 'password'.

Choose the initial address.

All interactions with the blockchain will be displayed in the section titled "Recent Transactions."

At the top, navigate to the smart contracts section and leave it alone; we'll return to this later.

Now, let's write a simple program to demonstrate the functionality that our tool will require.
Create a new class with any name you like and inherit the ISmartToolGenerator interface.

For the purposes of this tutorial, you will not be writing IoT code, as the entire process will be carried out on your PC. Therefore, let us develop a system that retrieves the temperature from an IoT device (we will substitute static data for the temperature) and stores it on the blockchain.

We'll indicate the location of the code with attributes (IoTDevice or Stratis). If no attributes are specified for a field or method, the code will be executed on the main console.


Copy the below code and replace it in the class you have created inside the ToGenerate solution

    public class Temperature : ISmartToolGenerator
    {
        // Note: Stratis and IotDevice attributes indicate where the variables and methods will live
        // Methods can only access variables which live on the same platform, meaning a method with a Stratis attribute can only only access a variable with a Stratis attribute

        [Stratis]
        public int TemperatureReadings;

        [Stratis]
        public int CurrentTemperature;

        // The main function will contain the logic need between in order to integrate both Blockchain and IoT devices together.
        public void Main()
        {
            while(true)
            {
                //Retrieves the data from the Api, which will eventually live next to an IoT device
                var temp = GetTemperature();

                // Stores the data on the blockchain
                StoreTemperature(temp);

                // Waits 10 seconds before process is iterated
                Task.Delay(10000);
            }
        }

        [IoTDevice]
        public int GetTemperature()
        {
            // A random number is returned, in order to substitute the IoT data.
            Random rnd = new Random();
            return rnd.Next(50);
        }

        [Stratis]
        public void StoreTemperature(int tempReading)
        {
            // Update the temperature reading on the blockchain
            CurrentTemperature = tempReading;

            //Increase temperature reading
            TemperatureReadings += 1;
        }
    }

Resolve any missing references.
Then, right-click on the ToGenerate project and select Build.

Then right click once more and choose 'Open folder in file explorer.' Navigate to bin/debug/net5.0/ and copy the path to the 'ToGenerate.dll' file.





Then right click again and click 'Open folder in file explorer'
Goto bin/debug/net5.0/ and copy the path to the 'ToGenerate.dll' file

Now run StratisTool.exe, and enter the dll path for the first question

Then enter the output path (optional: if not specified the folder generated will be placed in the bin next to the Generated.dll)

Then press Y in order to validate the smart contract and generate the bytecode which will be used to deploy the contract on the cirrus node.

If the contract compiles successfully copy the byte code and go back to the Cirrus Node that was set at the start of this tutorial.

Go to Smart contracts, click on 'Create Contract' and fill in the details

Then when it is executed wait until the transaction is confirmed and copy the address of the smart contract.

Open the console app folder and enter the contract address where specified.

Then open the IotApp project and run it. This is a simple api that can be placed on a raspberry pi, however for this demo we will run it on your PC

Go back to the console app and start the app
